using System;
using Blueprint.CodeGen;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Middleware
{
    /// <summary>
    /// An <see cref="IOperationExecutorBuilder" /> that will use an IoC registered <see cref="IApiOperationHandler{T}" /> to perform the execution,
    /// creating an instance from the IoC container of the pipeline and calling the <see cref="IApiOperationHandler{T}.Handle"/> method.
    /// </summary>
    public class ApiOperationHandlerExecutorBuilder : IOperationExecutorBuilder
    {
        private readonly Type _apiOperationHandlerType;
        private readonly string _foundAt;

        /// <summary>
        /// Creates a new instance of the <see cref="ApiOperationHandlerExecutorBuilder" /> that represents the given <see cref="ApiOperationDescriptor"/>.
        /// </summary>
        /// <param name="operation">The operation this builder handles.</param>
        /// <param name="apiOperationHandlerType">The type of the <see cref="IApiOperationHandler{T}"/> to be used in the pipeline.</param>
        /// <param name="foundAt">Where this builder was found, for diagnostics purposes.</param>
        public ApiOperationHandlerExecutorBuilder(ApiOperationDescriptor operation, Type apiOperationHandlerType, string foundAt)
        {
            Guard.NotNull(nameof(operation), operation);
            Guard.NotNull(nameof(apiOperationHandlerType), apiOperationHandlerType);
            Guard.NotNull(nameof(foundAt), foundAt);

            this.Operation = operation;
            this._apiOperationHandlerType = apiOperationHandlerType;
            this._foundAt = foundAt;
        }

        /// <inheritdoc />
        public ApiOperationDescriptor Operation { get; }

        /// <inheritdoc />
        public Variable Build(MiddlewareBuilderContext context)
        {
            var getInstanceFrame = context.VariableFromContainer(this._apiOperationHandlerType);

            // We must look for the _exact_ method call that corresponds to the operation type as
            // we support handlers that implement multiple IApiOperationHandler<T> interfaces
            var handlerInvokeCall = new MethodCall(
                this._apiOperationHandlerType,
                this._apiOperationHandlerType.GetMethod(
                    nameof(IApiOperationHandler<object>.Handle),
                    new[] { this.Operation.OperationType, typeof(ApiOperationContext) }));

            context.AppendFrames(
                getInstanceFrame,
                LogFrame.Information(
                    "Executing API operation with handler {HandlerType}",
                    new Variable(typeof(string), $"{getInstanceFrame.InstanceVariable}.GetType().Name")),
                handlerInvokeCall);

            return handlerInvokeCall.ReturnVariable;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this._foundAt;
        }
    }
}
