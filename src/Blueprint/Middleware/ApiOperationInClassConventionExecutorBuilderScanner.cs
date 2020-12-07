using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Blueprint.Compiler.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Middleware
{
    /// <summary>
    /// An <see cref="IOperationExecutorBuilderScanner" /> that searches for <see cref="IApiOperationHandler{T}" /> and creates corresponding
    /// <see cref="ApiOperationInClassConventionExecutorBuilder"/>s.
    /// </summary>
    public class ApiOperationInClassConventionExecutorBuilderScanner : IOperationExecutorBuilderScanner
    {
        private static readonly string[] _allowedMethodNames = { "Invoke", "InvokeAsync", "Execute", "ExecuteAsync", "Handle", "HandleAsync" };

        /// <inheritdoc />
        public IEnumerable<IOperationExecutorBuilder> FindHandlers(
            IServiceCollection services,
            Type operationType,
            IEnumerable<Assembly> scannedAssemblies)
        {
            foreach (var method in operationType.GetMethods())
            {
                if (_allowedMethodNames.Contains(method.Name))
                {
                    var typedOperation = operationType
                        .GetInterfaces()
                        .SingleOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReturn<>));

                    if (typedOperation != null)
                    {
                        var declaredReturnType = typedOperation.GetGenericArguments()[0];
                        var isTaskWrapped = method.ReturnType.Closes(typeof(Task<>)) || method.ReturnType.Closes(typeof(ValueTask<>));
                        var unwrappedReturnType = isTaskWrapped ? method.ReturnType.GetGenericArguments()[0] : method.ReturnType;

                        if (declaredReturnType != unwrappedReturnType)
                        {
                            throw new InvalidReturnTypeException(
                                $"Operation {operationType.Name} declares a return type of {declaredReturnType}, but the method {method.Name} has an incompatible return type of {method.ReturnType.Name}");
                        }
                    }

                    yield return new ApiOperationInClassConventionExecutorBuilder(operationType, method);
                }
            }
        }
    }
}
