using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Frames
{
    public class MethodCall : Frame
    {
        private readonly Type handlerType;
        private readonly MethodInfo methodInfo;

        private Variable target;

        public MethodCall(Type handlerType, string methodName) : this(handlerType, handlerType.GetMethod(methodName))
        {
        }

        public MethodCall(Type handlerType, MethodInfo methodInfo) : base(methodInfo.IsAsync())
        {
            this.handlerType = handlerType;
            this.methodInfo = methodInfo;

            var returnType = CorrectedReturnType(methodInfo.ReturnType);
            if (returnType != null)
            {
                if (returnType.IsValueTuple())
                {
                    var values = returnType.GetGenericArguments().Select(x => new Variable(x, this)).ToArray();

                    ReturnVariable = new TupleReturnVariable(returnType, values);
                }
                else
                {
                    var name = returnType.IsSimple() || returnType == typeof(object) || returnType == typeof(object[])
                        ? "result_of_" + methodInfo.Name
                        : Variable.DefaultArgName(returnType);

                    ReturnVariable = new Variable(returnType, name, this);
                }
            }

            var parameters = methodInfo.GetParameters();
            Arguments = new Variable[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                if (param.IsOut)
                {
                    var paramType = param.ParameterType.IsByRef ? param.ParameterType.GetElementType() : param.ParameterType;
                    Arguments[i] = new OutArgument(paramType, this);
                }
            }
        }

        /// <summary>
        /// The output variable of this method call, which may be <c>null</c> if the method has
        /// a <c>void</c> return type.
        /// </summary>
        public Variable ReturnVariable { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a call a method on the current object.
        /// </summary>
        public bool IsLocal { get; set; }

        /// <summary>
        /// The target variable of this method call, the variable that will have the method call
        /// executed on.
        /// </summary>
        public Variable Target
        {
            get => target;

            set
            {
                target = value;

                // Record this frame uses the target to propagate this association.
                AddUses(value);

                // Record the return variable, if one exists, has a dependency on the target
                // variable. This is obvious, but makes relationships even more explicit
                ReturnVariable?.Dependencies.Add(target);
            }
        }

        public Variable[] Arguments { get; }

        public DisposalMode DisposalMode { get; set; } = DisposalMode.UsingBlock;

        public static MethodCall For<T>(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);

            return new MethodCall(typeof(T), method);
        }

        public bool TrySetArgument(Variable variable)
        {
            var parameters = methodInfo.GetParameters().Select(x => x.ParameterType).ToArray();

            if (parameters.Count(x => variable.VariableType.CanBeCastTo(x)) != 1)
            {
                return false;
            }

            var index = Array.IndexOf(parameters, variable.VariableType);
            Arguments[index] = variable;

            return true;
        }

        public bool TrySetArgument(string parameterName, Variable variable)
        {
            var parameters = methodInfo.GetParameters().ToArray();
            var matching = parameters.FirstOrDefault(x =>
                variable.VariableType.CanBeCastTo(x.ParameterType) && x.Name == parameterName);

            if (matching == null)
            {
                return false;
            }

            var index = Array.IndexOf(parameters, matching);
            Arguments[index] = variable;

            return true;
        }

        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            var parameters = methodInfo.GetParameters().ToArray();
            for (var i = 0; i < parameters.Length; i++)
            {
                if (Arguments[i] != null)
                {
                    AddUses(Arguments[i]);

                    continue;
                }

                Arguments[i] = variables.FindVariable(parameters[i].ParameterType);
            }

            // If we do not have an explicit Target variable already and we need one, try and find it
            if (Target == null && !(methodInfo.IsStatic || IsLocal))
            {
                Target = variables.FindVariable(handlerType);
            }

            var invokeMethod = GetInvocationCode();

            var returnValue = string.Empty;

            if (IsAsync)
            {
                returnValue = method.AsyncMode == AsyncMode.ReturnFromLastNode ? "return " : "await ";
            }

            var isDisposable = false;
            if (ShouldAssignVariableToReturnValue(method))
            {
                returnValue = ReturnVariable.VariableType.IsValueTuple() ? $"{ReturnVariable} = {returnValue}" : $"var {ReturnVariable} = {returnValue}";
                isDisposable = ReturnVariable.VariableType.CanBeCastTo<IDisposable>();
            }

            if (isDisposable && DisposalMode == DisposalMode.UsingBlock)
            {
                writer.UsingBlock($"{returnValue}{invokeMethod}", w => next());
            }
            else
            {
                writer.Write($"{returnValue}{invokeMethod};");

                next();
            }
        }

        /// <summary>
        /// Code to invoke the method without any assignment to a variable.
        /// </summary>
        /// <returns></returns>
        public string InvocationCode()
        {
            return IsAsync ? "await " + GetInvocationCode() : GetInvocationCode();
        }

        /// <summary>
        /// Code to invoke the method and set a variable to the returned value.
        /// </summary>
        /// <returns></returns>
        public string AssignmentCode()
        {
            if (ReturnVariable == null)
            {
                throw new InvalidOperationException($"Method {this} does not have a return value");
            }

            return IsAsync
                ? $"var {ReturnVariable} = await {InvocationCode()}"
                : $"var {ReturnVariable} = {InvocationCode()}";
        }

        public override bool CanReturnTask()
        {
            return IsAsync;
        }

        public override string ToString()
        {
            return InvocationCode();
        }

        private static Type CorrectedReturnType(Type type)
        {
            if (type == typeof(Task) || type == typeof(ValueTask) || type == typeof(void))
            {
                return null;
            }

            if (type.CanBeCastTo<Task>())
            {
                return type.GetGenericArguments().First();
            }

            // have to do a manual check for ValueTask<T> as it doesn't inherit from ValueTask
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                return type.GetGenericArguments().First();
            }

            return type;
        }

        private bool ShouldAssignVariableToReturnValue(GeneratedMethod method)
        {
            if (ReturnVariable == null)
            {
                return false;
            }

            if (IsAsync && method.AsyncMode == AsyncMode.ReturnFromLastNode)
            {
                return false;
            }

            return true;
        }

        private string GetInvocationCode()
        {
            var methodName = methodInfo.Name;
            if (methodInfo.IsGenericMethod)
            {
                methodName += $"<{methodInfo.GetGenericArguments().Select(x => x.FullName).Join(", ")}>";
            }

            var callingCode = $"{methodName}({Arguments.Select(x => x.ArgumentDeclaration).Join(", ")})";
            var target = DetermineTarget();
            var invokeMethod = $"{target}{callingCode}";

            return invokeMethod;
        }

        private string DetermineTarget()
        {
            if (IsLocal)
            {
                return string.Empty;
            }

            var target = methodInfo.IsStatic
                ? handlerType.FullNameInCode()
                : Target.Usage;

            return target + ".";
        }
    }
}
