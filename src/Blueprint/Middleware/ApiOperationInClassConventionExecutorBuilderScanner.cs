using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Middleware
{
    /// <summary>
    /// An <see cref="IOperationExecutorBuilderScanner" /> that searches for <see cref="IApiOperationHandler{T}" /> and creates corresponding
    /// <see cref="ApiOperationInClassConventionExecutorBuilder"/>s.
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
                    if (AllowedMethodNames.Contains(method.Name))
                    {
                        yield return new ApiOperationInClassConventionExecutorBuilder(operation, method);
                    }
                }
            }
        }
    }
}
