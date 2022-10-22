using System;

namespace Blueprint.Configuration;

/// <summary>
/// A Blueprint "Host", an external interface that has the ability to run Blueprint operations, such as a HTTP host
/// or a background job host.
/// </summary>
public interface IBlueprintHost
{
    /// <summary>
    /// Gets a value indicating whether the <see cref="Type" /> is a supported operation and should therefore
    /// be included in the <see cref="ApiDataModel" /> that is being built.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Hosts are responsible for registering themselves as an <see cref="IBlueprintHost"/> and indicating what operation
    /// types they can support (i.e. the HTTP host would support those operations with a declared HTTP route or a
    /// background task processor would only care operations marked with an IBackgroundTask interface).
    /// </para>
    /// <para>
    /// If multiple hosts are registered, it only takes a single host to return <c>true</c> to include
    /// that operation.
    /// </para>
    /// </remarks>
    /// <param name="operationType">The type of operation to check.</param>
    /// <returns>Whether the operation type is supported.</returns>
    bool IsSupported(Type operationType);
}
