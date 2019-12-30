using System;
using Blueprint.Api.CodeGen;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// An <see cref="IOperationExecutorBuilder" /> that will use an IoC registered <see cref="IApiOperationHandler{T}" /> to perform the execution,
    /// creating an instance from the IoC container of the pipeline and calling the <see cref="IApiOperationHandler{T}.Invoke"/> method.
    /// </summary>
    public class ApiOperationHandlerExecutorBuilder : IOperationExecutorBuilder
    {
        private readonly Type apiOperationHandlerType;

        /// <summary>
        /// Creates a new instance of the <see cref="ApiOperationHandlerExecutorBuilder" /> that represents the given <see cref="ApiOperationDescriptor"/>.
        /// </summary>
        /// <param name="operation">The operation this builder handles.</param>
        /// <param name="apiOperationHandlerType">The type of the <see cref="IApiOperationHandler{T}"/> to be used in the pipeline.</param>
        public ApiOperationHandlerExecutorBuilder(ApiOperationDescriptor operation, Type apiOperationHandlerType)
        {
            Operation = operation;
            this.apiOperationHandlerType = apiOperationHandlerType;
        }

        /// <inheritdoc />
        public ApiOperationDescriptor Operation { get; }

        /// <inheritdoc />
        public Variable Build(MiddlewareBuilderContext context)
        {
            var getInstanceFrame = context.VariableFromContainer(apiOperationHandlerType);
            var handlerInvokeCall = new MethodCall(apiOperationHandlerType, nameof(IApiOperationHandler<IApiOperation>.Invoke));

            context.AppendFrames(
                getInstanceFrame,
                LogFrame.Debug("Executing API operation. handler_type={0}", $"{getInstanceFrame.InstanceVariable}.GetType().Name"),
                handlerInvokeCall);

            return handlerInvokeCall.ReturnVariable;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{apiOperationHandlerType} handles {Operation}";
        }
    }
}
