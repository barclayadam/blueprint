using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Middleware;

/// <summary>
/// An <see cref="IOperationExecutorBuilderScanner" /> that searches for <see cref="IApiOperationHandler{T}" /> and creates corresponding
/// <see cref="ApiOperationInClassConventionExecutorBuilder"/>s.
/// </summary>
public class ApiOperationInClassConventionExecutorBuilderScanner : IOperationExecutorBuilderScanner
{
    private static readonly string[] _allowedMethodNames = { "Invoke", "InvokeAsync", "Execute", "ExecuteAsync", "Handle", "HandleAsync" };

    /// <inheritdoc />
    public void FindHandlers(ScannerContext scannerContext)
    {
        foreach (var op in scannerContext.Operations)
        {
            var operationType = op.OperationType;

            foreach (var method in operationType.GetMethods())
            {
                if (!_allowedMethodNames.Contains(method.Name))
                {
                    continue;
                }

                var typedOperation = operationType
                    .GetInterfaces()
                    .SingleOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReturn<>));

                if (typedOperation != null)
                {
                    var declaredReturnType = typedOperation.GetGenericArguments()[0];
                    var unwrappedReturnType = UnwrapReturnTypeFromTask(method.ReturnType);

                    if (declaredReturnType != unwrappedReturnType)
                    {
                        throw new InvalidReturnTypeException(
                            $"Operation {operationType.Name} declares a return type of {declaredReturnType}, but the method {method.Name} has an incompatible return type of {unwrappedReturnType.Name}");
                    }
                }

                scannerContext.RegisterHandler(op, new ApiOperationInClassConventionExecutorBuilder(operationType, method));
            }
        }
    }

    private static Type UnwrapReturnTypeFromTask(Type returnType)
    {
        // Cannot be a Task-like return
        if (!returnType.IsGenericType)
        {
            return returnType;
        }

        var genericTypeDefinition = returnType.GetGenericTypeDefinition();

        if (genericTypeDefinition == typeof(Task<>) || genericTypeDefinition == typeof(ValueTask<>))
        {
            return returnType.GetGenericArguments()[0];
        }

        // Is not task-like, return as-is
        return returnType;
    }
}
