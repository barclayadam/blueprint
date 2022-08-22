using System;
using System.Linq;
using Blueprint.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;
using Blueprint.Utilities;
using Microsoft.Extensions.Logging;

namespace Blueprint.Middleware;

/// <summary>
/// An <see cref="IOperationExecutorBuilder" /> that will use an IoC registered <see cref="IApiOperationHandler{T}" /> to perform the execution,
/// creating an instance from the IoC container of the pipeline and calling the <see cref="IApiOperationHandler{T}.Handle"/> method.
/// </summary>
public class ApiOperationHandlerExecutorBuilder : IOperationExecutorBuilder
{
    private static readonly EventId _apiOperationExecutorLogEvent = new EventId(2, "ApiOperationHandlerExecuting");

    private readonly Type _operationType;
    private readonly Type _iocServiceType;
    private readonly Type _apiOperationHandlerType;
    private readonly Type _handledOperationType;
    private readonly string _foundAt;

    /// <summary>
    /// Creates a new instance of the <see cref="ApiOperationHandlerExecutorBuilder" /> that represents the given <see cref="ApiOperationDescriptor"/>.
    /// </summary>
    /// <param name="operationType">The operation this builder handles.</param>
    /// <param name="iocServiceType">The type that we should grab form the IoC container, which may be different from handler type
    /// if it is, for example, registered as IApiOperationHandler{T} (the built-in IoC container cannot construct instances of classes not
    /// explicitly registered)</param>
    /// <param name="apiOperationHandlerType">The type of the <see cref="IApiOperationHandler{T}"/> to be used in the pipeline.</param>
    /// <param name="handledOperationType">The actual type the handler has _declared_ it handles.</param>
    /// <param name="foundAt">Where this builder was found, for diagnostics purposes.</param>
    public ApiOperationHandlerExecutorBuilder(
        Type operationType,
        Type iocServiceType,
        Type apiOperationHandlerType,
        Type handledOperationType,
        string foundAt)
    {
        Guard.NotNull(nameof(operationType), operationType);
        Guard.NotNull(nameof(iocServiceType), iocServiceType);
        Guard.NotNull(nameof(apiOperationHandlerType), apiOperationHandlerType);
        Guard.NotNull(nameof(handledOperationType), handledOperationType);
        Guard.NotNull(nameof(foundAt), foundAt);

        this._operationType = operationType;
        this._iocServiceType = iocServiceType;
        this._apiOperationHandlerType = apiOperationHandlerType;
        this._handledOperationType = handledOperationType;
        this._foundAt = foundAt;
    }

    /// <summary>
    /// Gets the handler <see cref="Type" /> that will be executed for the operation this builder
    /// represents.
    /// </summary>
    public Type ApiOperationHandlerType => this._apiOperationHandlerType;

    /// <inheritdoc/>
    public Type HandlerType => this._apiOperationHandlerType;

    /// <inheritdoc />
    public Variable Build(MiddlewareBuilderContext context, ExecutorReturnType executorReturnType)
    {
        var getInstanceFrame = context.VariableFromContainer(this._iocServiceType);

        // We must look for the _exact_ method call that corresponds to the operation type as
        // we support handlers that implement multiple IApiOperationHandler<T> interfaces
        var handlerInvokeCall = new MethodCall(
            this._iocServiceType,
            this._iocServiceType.GetMethods().First(m => m.Name == nameof(IApiOperationHandler<object>.Handle)))
        {
            IgnoreReturnVariable = executorReturnType == ExecutorReturnType.NoReturn,
        };

        var invocationFrames = new Frame[]
        {
            getInstanceFrame,
            LogFrame.Debug(
                _apiOperationExecutorLogEvent,
                "Executing API operation {OperationType} with handler {HandlerType}",
                ReflectionUtilities.PrettyTypeName(context.Descriptor.OperationType),
                new Variable(typeof(string), $"{getInstanceFrame.InstanceVariable}.GetType().Name")),
            handlerInvokeCall,
        };

        // If it is not directly assignable then we need to do a runtime check and cast. This handles the case
        // of a concrete handler for an interface operation (i.e. operation is IGenericMessage and the handler
        // is IApiOperationHandler<ConcreteMessage> where ConcreteMessage : IGenericMessage)
        if (this._handledOperationType.IsAssignableFrom(this._operationType) == false)
        {
            var operationVariable = context.FindVariable(this._operationType);

            handlerInvokeCall.TrySetArgument(new CastVariable(
                operationVariable,
                this._handledOperationType));

            context.AppendFrames(
                new IfBlock($"{operationVariable} is {this._handledOperationType.FullNameInCode()}", invocationFrames));

            return null;
        }

        // We are explicit about setting the operation argument as it may be that the operation type is not
        // exactly the same (inheritance) and would therefore not be found by the variable system
        handlerInvokeCall.Arguments[0] = context.FindVariable(this._operationType);

        context.AppendFrames(invocationFrames);

        return handlerInvokeCall.ReturnVariable;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return this._foundAt;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return this.Equals((ApiOperationHandlerExecutorBuilder)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = this._operationType.GetHashCode();
            hashCode = (hashCode * 397) ^ this._apiOperationHandlerType.GetHashCode();
            hashCode = (hashCode * 397) ^ this._handledOperationType.GetHashCode();
            return hashCode;
        }
    }

    private bool Equals(ApiOperationHandlerExecutorBuilder other)
    {
        return this._operationType == other._operationType &&
               this._apiOperationHandlerType == other._apiOperationHandlerType &&
               this._handledOperationType == other._handledOperationType;
    }
}