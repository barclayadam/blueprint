using System;
using System.Reflection;
using System.Threading.Tasks;
using Blueprint.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;
using Blueprint.Utilities;
using Microsoft.Extensions.Logging;

namespace Blueprint.Middleware;

/// <summary>
/// An <see cref="IOperationExecutorBuilder" /> that will use a public method that exists on the operation class
/// itself.
/// </summary>
public class ApiOperationInClassConventionExecutorBuilder : IOperationExecutorBuilder
{
    private static readonly EventId _apiOperationExecutorLogEvent = new EventId(3, "OperationExecuting");

    private readonly Type _operationType;
    private readonly MethodInfo _method;

    /// <summary>
    /// Creates a new instance of the <see cref="ApiOperationInClassConventionExecutorBuilder" /> that represents the given <see cref="ApiOperationDescriptor"/>.
    /// </summary>
    /// <param name="operationType">The operation this builder handles.</param>
    /// <param name="method">The method that is to be executed.</param>
    public ApiOperationInClassConventionExecutorBuilder(Type operationType, MethodInfo method)
    {
        this._operationType = operationType;
        this._method = method;
    }

    /// <inheritdoc/>
    public Type HandlerType => this._operationType;

    /// <inheritdoc />
    public Variable Build(MiddlewareBuilderContext context, ExecutorReturnType executorReturnType)
    {
        // We rely on the compiler infrastructure to make the correct calls, to the correct type (i.e. the actual
        // operation), and to fill in the parameters of that method as required.
        var handlerInvokeCall = new MethodCall(context.Descriptor.OperationType, this._method)
        {
            IgnoreReturnVariable = executorReturnType == ExecutorReturnType.NoReturn,
        };

        // Note that although we know the handler type at compile time, we still specify it as a
        // parameter to logging so that it is output as a structured value (as it changes between
        // invocations)
        context.AppendFrames(
            LogFrame.Debug(
                _apiOperationExecutorLogEvent,
                "Executing API operation {OperationType} with inline handler",
                ReflectionUtilities.PrettyTypeName(context.Descriptor.OperationType)),
            handlerInvokeCall);

        // We have a void, or a Task (i.e. async with no return) so we will convert to a 'NoResult'
        if (handlerInvokeCall.ReturnVariable == null || handlerInvokeCall.ReturnVariable.VariableType == typeof(Task))
        {
            var emptyResultCreation = new VariableCreationFrame(
                typeof(NoResultOperationResult),
                $"{typeof(NoResultOperationResult).FullNameInCode()}.{nameof(NoResultOperationResult.Instance)};");

            context.AppendFrames(emptyResultCreation);

            return emptyResultCreation.CreatedVariable;
        }

        return handlerInvokeCall.ReturnVariable;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this._operationType.Name}.{this._method.Name}";
    }
}