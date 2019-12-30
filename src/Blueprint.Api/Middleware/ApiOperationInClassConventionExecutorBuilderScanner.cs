using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blueprint.Api.CodeGen;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// An <see cref="IOperationExecutorBuilderScanner" /> that searches for <see cref="IApiOperationHandler{T}" /> and creates corresponding
    /// <see cref="ApiOperationInClassConventionExecutorBuilderScanner.ApiOperationInClassConventionExecutorBuilder"/>s.
    /// </summary>
    public class ApiOperationInClassConventionExecutorBuilderScanner : IOperationExecutorBuilderScanner
    {
        private static readonly string[] AllowedMethodNames = {"Invoke", "InvokeAsync", "Execute", "ExecuteAsync", "Handle", "HandleAsync"};

        /// <inheritdoc />
        public IEnumerable<IOperationExecutorBuilder> FindHandlers(
            IServiceCollection services,
            IEnumerable<ApiOperationDescriptor> operations)
        {
            foreach (var operation in operations)
            {
                foreach (var method in operation.OperationType.GetMethods())
                {
                    if (AllowedMethodNames.Contains(method.Name) && method.ReturnType != typeof(void))
                    {
                        yield return new ApiOperationInClassConventionExecutorBuilder(operation, method);
                    }
                }
            }
        }

        private class ApiOperationInClassConventionExecutorBuilder : IOperationExecutorBuilder
        {
            private readonly MethodInfo method;

            public ApiOperationInClassConventionExecutorBuilder(ApiOperationDescriptor operation, MethodInfo method)
            {
                Operation = operation;
                this.method = method;
            }

            public ApiOperationDescriptor Operation { get; }

            public Variable Build(MiddlewareBuilderContext context)
            {
                // We rely on the compiler infrastructure to make the correct calls, to the correct type (i.e. the actual
                // operation), and to fill in the parameters of that method as required.
                var handlerInvokeCall = new MethodCall(context.Descriptor.OperationType, method);

                context.AppendFrames(
                    LogFrame.Debug($"Executing API operation. handler_type={method.DeclaringType.Name}"),
                    handlerInvokeCall);

                return handlerInvokeCall.ReturnVariable;
            }

            public override string ToString()
            {
                return $"{method.DeclaringType.Name}.{method.Name} handles {Operation}";
            }
        }
    }
}
