using System;
using System.Collections.Generic;

namespace Blueprint.Middleware;

/// <summary>
/// An <see cref="IOperationExecutorBuilderScanner" /> that searches for <see cref="IApiOperationHandler{T}" /> and creates corresponding
/// <see cref="ApiOperationHandlerExecutorBuilder"/>s.
/// </summary>
public class ApiOperationHandlerExecutorBuilderScanner : IOperationExecutorBuilderScanner
{
    /// <inheritdoc />
    public void FindHandlers(ScannerContext scannerContext)
    {
        var implementationsInIoC = new List<(Type OpType, Type ServiceType)>();

        // First, loop around service collection to find any handlers that have been registered. This should quickly
        // eliminate most services, and avoids any sort of double-looping trying to find specific handlers for
        // each operation individually.
        foreach (var s in scannerContext.Services)
        {
            var serviceType = s.ServiceType;

            // Registered directly as an IApiOperationHandler (i.e. services.AddScoped<IApiOperationHandler<SomeOperation>, SomeHandler>())
            if (serviceType.IsInterface &&
                serviceType.IsGenericType &&
                serviceType.GetGenericTypeDefinition() == typeof(IApiOperationHandler<>))
            {
                implementationsInIoC.Add((serviceType.GetGenericArguments()[0], serviceType));
            }

            // Registered as a concrete type that implements one or more IApiOperationHandler interfaces
            // (i.e. services.AddScoped<SomeHandler>() where SomeHandler implements IApiOperationHandler<SomeOperation>)
            if (serviceType.IsAssignableTo(typeof(IApiOperationHandler<>)))
            {
                foreach (var i in serviceType.GetInterfaces())
                {
                    if (i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IApiOperationHandler<>))
                    {
                        implementationsInIoC.Add((i.GetGenericArguments()[0], serviceType));
                    }
                }
            }
        }

        foreach (var op in scannerContext.Operations)
        {
            var operationType = op.OperationType;

            foreach (var impl in implementationsInIoC)
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
                if (impl.OpType == operationType || impl.OpType.IsAssignableTo(operationType) || impl.OpType.IsAssignableFrom(operationType) )
                {
                    scannerContext.RegisterHandler(
                        op,
                        new ApiOperationHandlerExecutorBuilder(
                            operationType,
                            impl.ServiceType,
                            impl.ServiceType,
                            impl.OpType,
                            $"IoC as {impl.ServiceType}"));
                }
            }
        }
    }
}
