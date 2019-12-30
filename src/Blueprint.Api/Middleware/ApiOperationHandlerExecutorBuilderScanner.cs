using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Api.CodeGen;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// An <see cref="IOperationExecutorBuilderScanner" /> that searches for <see cref="IApiOperationHandler{T}" /> and creates corresponding
    /// <see cref="ApiOperationHandlerExecutorBuilder"/>s.
    /// </summary>
    public class ApiOperationHandlerExecutorBuilderScanner : IOperationExecutorBuilderScanner
    {
        /// <inheritdoc />
        public IEnumerable<IOperationExecutorBuilder> FindHandlers(
            IServiceCollection services,
            IEnumerable<ApiOperationDescriptor> operations)
        {
            foreach (var operation in operations)
            {
                var apiOperationHandlerType = typeof(IApiOperationHandler<>).MakeGenericType(operation.OperationType);

                // First, check if there has already been a handler registered for this operation in the service
                // collection
                if (services.Any(d => apiOperationHandlerType.IsAssignableFrom(d.ServiceType)))
                {
                    yield return new ApiOperationHandlerExecutorBuilder(operation, apiOperationHandlerType);
                }

                // If not, we try to manually find and register the handler, looking for an implementation of
                // IApiOperationHandler<{OperationType}> alongside the operation (i.e. in the same assembly)
                var apiOperationHandler = FindApiOperationHandler(operation, apiOperationHandlerType);

                if (apiOperationHandler != null)
                {
                    services.AddScoped(apiOperationHandlerType, apiOperationHandler);
                    yield return new ApiOperationHandlerExecutorBuilder(operation, apiOperationHandlerType);
                }
            }
        }

        private static Type FindApiOperationHandler(ApiOperationDescriptor apiOperationDescriptor, Type apiOperationHandlerType)
        {
            return apiOperationDescriptor.OperationType.Assembly.GetExportedTypes().SingleOrDefault(apiOperationHandlerType.IsAssignableFrom);
        }
        /// <summary>
        /// An <see cref="IOperationExecutorBuilder" /> that will use an IoC registered <see cref="IApiOperationHandler{T}" /> to perform the execution,
        /// creating an instance from the IoC container of the pipeline and calling the <see cref="IApiOperationHandler{T}.Invoke"/> method.
        /// </summary>
        private class ApiOperationHandlerExecutorBuilder : IOperationExecutorBuilder
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
}
