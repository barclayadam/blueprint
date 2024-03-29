﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Frames;

/// <summary>
/// A <see cref="Frame" /> that represents a method call, handling async vs non-async, disposable return values,
/// finding arguments and dealing with local vs static.
/// </summary>
public class MethodCall : Frame
{
    private readonly Type _handlerType;
    private readonly MethodInfo _methodInfo;
    private readonly ParameterInfo[] _parameters;

    private Variable _target;

    /// <summary>
    /// Initialises a new instance of the <see cref="MethodCall" /> class
    /// representing a call to a method on the type <paramref name="handlerType" />
    /// with the given name.
    /// </summary>
    /// <remarks>
    /// A method must exist with exactly the given name, that is a public
    /// instance method.
    /// </remarks>
    /// <param name="handlerType">The class the method belongs to.</param>
    /// <param name="methodName">The exact name of the method to call.</param>
    public MethodCall(Type handlerType, string methodName)
        : this(handlerType, handlerType.GetMethod(methodName))
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="MethodCall" /> class
    /// representing a call to a method on the type <paramref name="handlerType" />.
    /// </summary>
    /// <param name="handlerType">The class the method belongs to.</param>
    /// <param name="methodInfo">The method to call.</param>
    public MethodCall(Type handlerType, MethodInfo methodInfo)
        : base(methodInfo.IsAsync())
    {
        this._handlerType = handlerType;
        this._methodInfo = methodInfo;
        this._parameters = methodInfo.GetParameters();

        var returnType = CorrectedReturnType(methodInfo.ReturnType);
        if (returnType != null)
        {
            if (returnType.IsValueTuple())
            {
                var values = returnType.GetGenericArguments().Select(x => new Variable(x, this)).ToArray();

                this.ReturnVariable = new TupleReturnVariable(returnType, values);
            }
            else
            {
                var name = returnType.IsSimple() || returnType == typeof(object) || returnType == typeof(object[])
                    ? "result_of_" + methodInfo.Name
                    : Variable.DefaultName(returnType);

                this.ReturnVariable = new Variable(returnType, name, this);
            }
        }

        this.Arguments = new Variable[this._parameters.Length];
        for (var i = 0; i < this._parameters.Length; i++)
        {
            var param = this._parameters[i];
            if (param.IsOut)
            {
                var paramType = param.ParameterType.IsByRef ? param.ParameterType.GetElementType() : param.ParameterType;
                this.Arguments[i] = new OutArgument(paramType, this);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to ignore the return variable of this <see cref="MethodCall" /> (if there
    /// is one) and therefore NOT generate a variable assignment.
    /// </summary>
    public bool IgnoreReturnVariable { get; set; }

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
        get => this._target;

        set
        {
            this._target = value;

            // Record this frame uses the target to propagate this association.
            this.AddUses(value);

            // Record the return variable, if one exists, has a dependency on the target
            // variable. This is obvious, but makes relationships even more explicit
            this.ReturnVariable?.Dependencies.Add(this._target);
        }
    }

    /// <summary>
    /// The variable arguments to this method call, positioned in the same
    /// as the source of the method being called.
    /// </summary>
    public Variable[] Arguments { get; }

    /// <summary>
    /// Determines how the return variable of this method call should be disposed,
    /// if applicable.
    /// </summary>
    public DisposalMode DisposalMode { get; set; } = DisposalMode.UsingBlock;

    /// <summary>
    /// Creates a new <see cref="MethodCall" /> frame that is represented in
    /// the given <see cref="Expression{T}" />
    /// </summary>
    /// <remarks>
    /// The arguments to the method are completely ignored.
    /// </remarks>
    /// <typeparam name="T">The type from which an instance method will be called.</typeparam>
    /// <param name="expression">An expression (with default arguments filled) that
    /// represents a call to the method to use.</param>
    /// <returns>A new <see cref="MethodCall" />.</returns>
    public static MethodCall For<T>(Expression<Action<T>> expression)
    {
        var method = ReflectionHelper.GetMethod(expression);

        return new MethodCall(typeof(T), method);
    }

    /// <summary>
    /// Tries to set an argument of this method call, finding the single
    /// argument that matches the given variable's type.
    /// </summary>
    /// <param name="variable">The variable to set as an argument.</param>
    /// <returns>Whether the call succeeded, meaning there was exactly one argument
    /// with a matching type.</returns>
    public bool TrySetArgument(Variable variable)
    {
        var parameterTypes = this._parameters.Select(x => x.ParameterType).ToArray();

        if (parameterTypes.Count(x => variable.VariableType.CanBeCastTo(x)) != 1)
        {
            return false;
        }

        var index = Array.IndexOf(parameterTypes, variable.VariableType);
        this.Arguments[index] = variable;

        return true;
    }

    /// <summary>
    /// Tries to set an argument of this method call, finding the single
    /// argument that matches the given variable's type and name (case sensitive).
    /// </summary>
    /// <param name="parameterName">The case-sensitive name of the parameter.</param>
    /// <param name="variable">The variable to set as an argument.</param>
    /// <returns>Whether the call succeeded, meaning there was exactly one argument
    /// with a matching type and name.</returns>
    public bool TrySetArgument(string parameterName, Variable variable)
    {
        var matching = this._parameters.FirstOrDefault(x =>
            variable.VariableType.CanBeCastTo(x.ParameterType) && x.Name == parameterName);

        if (matching == null)
        {
            return false;
        }

        var index = Array.IndexOf(this._parameters, matching);
        this.Arguments[index] = variable;

        return true;
    }

    /// <inheritdoc />
    public override bool CanReturnTask()
    {
        return this.IsAsync;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var writer = new SourceWriter();
        this.AppendInvocationCode(writer);

        return this.IsAsync ? "await " + writer.Code() : writer.Code();
    }

    /// <inheritdoc />
    protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
    {
        for (var i = 0; i < this._parameters.Length; i++)
        {
            if (this.Arguments[i] != null)
            {
                this.AddUses(this.Arguments[i]);

                continue;
            }

            this.Arguments[i] = variables.FindVariable(this._parameters[i].ParameterType);
        }

        // If we do not have an explicit Target variable already and we need one, try and find it
        if (this.Target == null && !(this._methodInfo.IsStatic || this.IsLocal))
        {
            this.Target = variables.FindVariable(this._handlerType);
        }

        var shouldAssign = this.ShouldAssignVariableToReturnValue(method);
        var isDisposable = shouldAssign && this.ReturnVariable.VariableType.CanBeCastTo<IDisposable>();
        var requiresUsingBlock = isDisposable && this.DisposalMode == DisposalMode.UsingBlock;

        writer.Indent();

        if (requiresUsingBlock)
        {
            writer.Append("using (");
        }

        var callConvention = this.IsAsync ? method.AsyncMode == AsyncMode.ReturnFromLastNode ? "return " : "await " : string.Empty;

        if (shouldAssign)
        {
            if (this.ReturnVariable.VariableType.IsValueTuple())
            {
                writer.Append(this.ReturnVariable.ToString()).Append(" = ").Append(callConvention);
            }
            else
            {
                writer.Append("var ").Append(this.ReturnVariable.ToString()).Append(" = ").Append(callConvention);
            }
        }
        else
        {
            writer.Append(callConvention);
        }

        this.AppendInvocationCode(writer);

        if (isDisposable && this.DisposalMode == DisposalMode.UsingBlock)
        {
            // We have already written the using statement out to the writer, we just need a block
            // opener to increase indentation level with closing parenthesis
            writer.Append(')');
            writer.Block(string.Empty);
            next();
            writer.FinishBlock();
        }
        else
        {
            // Finish off the current line.
            writer.Append(';');
            writer.WriteLine(string.Empty);
            next();
        }
    }

    private static Type CorrectedReturnType(Type type)
    {
        if (type == typeof(Task) || type == typeof(ValueTask) || type == typeof(void))
        {
            return null;
        }

        if (!type.IsGenericType)
        {
            return type;
        }

        // Unwrap Task-like returns to have the return type be the _inner_ type
        var genericTypeDefinition = type.GetGenericTypeDefinition();

        if (genericTypeDefinition == typeof(ValueTask<>) || genericTypeDefinition == typeof(Task<>))
        {
            return type.GetGenericArguments()[0];
        }

        return type;
    }

    private bool ShouldAssignVariableToReturnValue(GeneratedMethod method)
    {
        if (this.ReturnVariable == null || this.IgnoreReturnVariable)
        {
            return false;
        }

        if (this.IsAsync && method.AsyncMode == AsyncMode.ReturnFromLastNode)
        {
            return false;
        }

        return true;
    }

    private void AppendInvocationCode(ISourceWriter sourceWriter)
    {
        // If this is not a local call we need to output either "ClassName." for a static method
        // invocation or "targetVar." otherwise
        if (!this.IsLocal)
        {
            sourceWriter.Append(this._methodInfo.IsStatic
                ? this._handlerType.FullNameInCode()
                : this.Target.Usage);

            sourceWriter.Append('.');
        }

        sourceWriter.Append(this._methodInfo.Name);

        // Write generic arguments if necessary (i.e. <int, object, string>)
        if (this._methodInfo.IsGenericMethod)
        {
            var genericArguments = this._methodInfo.GetGenericArguments();

            sourceWriter.Append('<');

            for (var i = 0; i < genericArguments.Length; i++)
            {
                sourceWriter.Append(genericArguments[i].FullNameInCode());

                if (i != genericArguments.Length - 1)
                {
                    sourceWriter.Append(", ");
                }
            }

            sourceWriter.Append('>');
        }

        // Write arguments
        sourceWriter.Append('(');

        for (var i = 0; i < this.Arguments.Length; i++)
        {
            sourceWriter.Append(this.Arguments[i].ArgumentDeclaration);

            if (i != this.Arguments.Length - 1)
            {
                sourceWriter.Append(", ");
            }
        }

        sourceWriter.Append(')');
    }
}
