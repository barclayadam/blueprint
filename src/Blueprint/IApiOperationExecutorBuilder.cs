using System;
using Blueprint.Configuration;

namespace Blueprint;

/// <summary>
/// A strategy factory that is used to determine how, at runtime, we should build and / or load an
/// <see cref="IApiOperationExecutor" /> from a configured <see cref="ApiDataModel" />.
/// </summary>
public interface IApiOperationExecutorBuilder
{
    /// <summary>
    /// Creates a new strategy from the given <see cref="ApiDataModel" />
    /// </summary>
    /// <returns>A new <see cref="IApiOperationExecutor" />.</returns>
    public IApiOperationExecutor Build();
}
