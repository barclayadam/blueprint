using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Blueprint;

/// <summary>
/// Provides a data model for an exposed API, a description of the resources that are being exposed, plus
/// the service and entity operations that have been registered.
/// </summary>
public class ApiDataModel
{
    private readonly Dictionary<Type, ApiOperationDescriptor> _allOperations = new();

    /// <summary>
    /// Gets all registered operations.
    /// </summary>
    public IEnumerable<ApiOperationDescriptor> Operations => this._allOperations.Values;

    /// <summary>
    /// Given a type that represents an API operation will construct a new <see cref="ApiOperationContext"/> that represents
    /// that operation.
    /// </summary>
    /// <param name="serviceProvider">The service provider under which the operation will execute.</param>
    /// <param name="type">The API operation to construct a context for.</param>
    /// <param name="token">A cancellation token to indicate the operation should stop.</param>
    /// <returns>A new <see cref="ApiOperationContext"/> representing the given type.</returns>
    public ApiOperationContext CreateOperationContext(IServiceProvider serviceProvider, Type type, CancellationToken token)
    {
        if (this._allOperations.TryGetValue(type, out var operation))
        {
            return new ApiOperationContext(serviceProvider, this, operation, token);
        }

        throw new InvalidOperationException($"Cannot find a registered operation of the type '{type.Name}'.");
    }

    /// <summary>
    /// Given a configured operation instance will create a new <see cref="ApiOperationContext" />.
    /// </summary>
    /// <param name="serviceProvider">The service provider under which the operation will execute.</param>
    /// <param name="operation">The configured operation instance.</param>
    /// <param name="token">A cancellation token to indicate the operation should stop.</param>
    /// <returns>A new <see cref="ApiOperationContext"/> representing the given operation.</returns>
    public ApiOperationContext CreateOperationContext(
        IServiceProvider serviceProvider,
        object operation,
        CancellationToken token)
    {
        return new ApiOperationContext(serviceProvider, this, operation, token);
    }

    /// <summary>
    /// Registers the given operation descriptor.
    /// </summary>
    /// <param name="descriptor">The descriptor to register, must be non-null.</param>
    public void RegisterOperation(ApiOperationDescriptor descriptor)
    {
        Guard.NotNull(nameof(descriptor), descriptor);

        this._allOperations[descriptor.OperationType] = descriptor;
    }

    /// <summary>
    /// Finds the <see cref="ApiOperationDescriptor" /> for an operation of the given type, which may be a concrete
    /// implementation of a registered interface-based operation.
    /// </summary>
    /// <param name="operationType">The type of operation to search for.</param>
    /// <returns>The <see cref="ApiOperationDescriptor" />.</returns>
    /// <exception cref="InvalidOperationException">If no operation descriptor exists.</exception>
    public ApiOperationDescriptor FindOperation(Type operationType)
    {
        if (!this.TryFindOperation(operationType, out var found))
        {
            throw new InvalidOperationException(@$"Could not find an operation of the type {operationType.FullName}.

Make sure that it has been correctly registered through the use of the .Operations(...) configuration method at startup.

Operations will be found polymorphically, meaning an operation could be registered as an interface with the model but found as a concrete implementation type at runtime");
        }

        return found;
    }

    /// <summary>
    /// Finds the <see cref="ApiOperationDescriptor" /> for an operation of the given type, which may be a concrete
    /// implementation of a registered interface-based operation.
    /// </summary>
    /// <param name="operationType">The type of operation to search for.</param>
    /// <param name="descriptor">The <see cref="ApiOperationDescriptor" />, or <c>null</c> if no such operation exists.</param>
    /// <returns>Whether an operation was found.</returns>
    public bool TryFindOperation(Type operationType, out ApiOperationDescriptor descriptor)
    {
        var found = this.Operations.SingleOrDefault(d => d.OperationType == operationType);

        if (found == null)
        {
            descriptor = default;
            return false;
        }

        descriptor = found;
        return true;
    }
}
