using System;
using System.Collections.Generic;
using System.Reflection;
using Blueprint.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Middleware;

/// <summary>
/// An <see cref="IOperationExecutorBuilderScanner" /> that searches for <see cref="IApiOperationHandler{T}" /> and creates corresponding
/// <see cref="ApiOperationHandlerExecutorBuilder"/>s.
/// </summary>
public class ApiOperationHandlerExecutorBuilderScanner : IOperationExecutorBuilderScanner
{
    /// <inheritdoc />
    public IEnumerable<IOperationExecutorBuilder> FindHandlers(
        IServiceCollection services,
        Type operationType,
        IEnumerable<Assembly> scannedAssemblies)
    {
        var found = new HashSet<IOperationExecutorBuilder>();

        // First, check if there has already been a handler registered for this operation in the service
        // collection
        foreach (var s in services)
        {
            var implementationType = s.ImplementationType ?? s.ImplementationInstance?.GetType();

            if (ImplementsHandler(operationType, implementationType, out var handledType))
            {
                found.Add(new ApiOperationHandlerExecutorBuilder(
                    operationType,
                    s.ServiceType,
                    implementationType,
                    handledType,
                    $"IoC as {implementationType}"));
            }
        }

        // If not, we try to manually find and register the handler, looking for an implementation of
        // IApiOperationHandler<{OperationType}> alongside the operation (i.e. in the same assembly)
        var foundTypes = FindApiOperationHandlers(operationType, scannedAssemblies);

        foreach (var (foundType, handledType) in foundTypes)
        {
            services.AddScoped(foundType, foundType);
            found.Add(new ApiOperationHandlerExecutorBuilder(
                operationType,
                foundType,
                foundType,
                handledType,
                $"Scanned {foundType}"));
        }

        return found;
    }

    private static IEnumerable<(Type HandlerType, Type OperationType)> FindApiOperationHandlers(
        Type operationType,
        IEnumerable<Assembly> scannedAssemblies)
    {
        // Most likely is the handler lives beside the operation, check that assembly first.
        foreach (var t in operationType.Assembly.GetExportedTypes())
        {
            if (ImplementsHandler(operationType, t, out var handledType))
            {
                yield return (t, handledType);
            }
        }

        // We also check other scanned assemblies.
        foreach (var a in scannedAssemblies)
        {
            foreach (var t in a.GetExportedTypes())
            {
                if (ImplementsHandler(operationType, t, out var handledType))
                {
                    yield return (t, handledType);
                }
            }
        }
    }

    private static bool ImplementsHandler(Type operationType, Type toCheck, out Type handledType)
    {
        if (toCheck == null)
        {
            handledType = null;
            return false;
        }

        if (toCheck.IsInterface && toCheck.IsOfGenericType(typeof(IApiOperationHandler<>), out var impl))
        {
            // Given:
            //   IOperation
            //   ConcreteOperation1 : IOperation
            //   ConcreteOperation2 : IOperation
            //
            // and a build for ConcreteOperation1 we want handlers that implement IApiOperationHandler<IOperation> OR
            // IApiOperationHandler<ConcreteOperation1> (i.e. any where <> is assignable to type).
            //
            // for a build for IOperation we want handlers that implement IApiOperationHandler<IOperation> OR
            // IApiOperationHandler<ConcreteOperation1> OR IApiOperationHandler<ConcreteOperation1> because any
            // one of them COULD, given runtime type, be executed (i.e. any where type assignable from <>)
            var implementationHandles = impl.GetGenericArguments()[0];

            if (operationType.IsAssignableFrom(implementationHandles) || implementationHandles.IsAssignableFrom(operationType))
            {
                handledType = implementationHandles;
                return true;
            }
        }

        foreach (var i in toCheck.GetInterfaces())
        {
            if (ImplementsHandler(operationType, i, out handledType))
            {
                return true;
            }
        }

        handledType = null;
        return false;
    }
}