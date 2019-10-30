using System;
using System.Collections.Generic;
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
        private readonly MethodInfo method;

        public MethodCall(Type handlerType, string methodName) : this(handlerType, handlerType.GetMethod(methodName))
        {
        }

        public MethodCall(Type handlerType, MethodInfo method) : base(method.IsAsync())
        {
            this.handlerType = handlerType;
            this.method = method;

            var returnType = CorrectedReturnType(method.ReturnType);
            if (returnType != null)
            {
                if (returnType.IsValueTuple())
                {
                    var values = returnType.GetGenericArguments().Select(x => new Variable(x, this)).ToArray();

                    ReturnVariable = new ValueTypeReturnVariable(returnType, values);
                }
                else
                {
                    var name = returnType.IsSimple() || returnType == typeof(object) || returnType == typeof(object[])
                        ? "result_of_" + method.Name
                        : Variable.DefaultArgName(returnType);

                    ReturnVariable = new Variable(returnType, name, this);
                }
            }

            var parameters = method.GetParameters();
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

        public Variable ReturnVariable { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a call a method on the current object.
        /// </summary>
        public bool IsLocal { get; set; }

        public Variable Target { get; set; }

        public Variable[] Arguments { get; }

        public DisposalMode DisposalMode { get; set; } = DisposalMode.UsingBlock;

        public static MethodCall For<T>(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);

            return new MethodCall(typeof(T), method);
        }

        public bool TrySetArgument(Variable variable)
        {
            var parameters = method.GetParameters().Select(x => x.ParameterType).ToArray();

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
            var parameters = method.GetParameters().ToArray();
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

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            var parameters = method.GetParameters().ToArray();
            for (var i = 0; i < parameters.Length; i++)
            {
                if (Arguments[i] != null)
                {
                    continue;
                }

                Arguments[i] = chain.FindVariable(parameters[i].ParameterType);
            }

            foreach (var variable in Arguments)
            {
                yield return variable;
            }

            if (method.IsStatic || IsLocal)
            {
                yield break;
            }

            if (Target == null)
            {
                Target = chain.FindVariable(handlerType);
            }

            yield return Target;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
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
                writer.UsingBlock($"{returnValue}{invokeMethod}", w => Next?.GenerateCode(method, writer));
            }
            else
            {
                writer.Write($"{returnValue}{invokeMethod};");

                Next?.GenerateCode(method, writer);
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
            if (type == typeof(Task) || type == typeof(void))
            {
                return null;
            }

            if (type.CanBeCastTo<Task>())
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
            var methodName = method.Name;
            if (method.IsGenericMethod)
            {
                methodName += $"<{method.GetGenericArguments().Select(x => x.FullName).Join(", ")}>";
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

            var target = method.IsStatic
                ? handlerType.FullNameInCode()
                : Target.Usage;

            return target + ".";
        }
    }
}
