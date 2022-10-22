using System;

namespace Blueprint.Configuration;

/// <summary>
/// An <see cref="IBlueprintHost" /> that marks any class that implements
/// <see cref="IQuery" /> or <see cref="ICommand" /> as a supported operation.
/// </summary>
public class CommandOrQueryHost : IBlueprintHost
{
    /// <inheritdoc />
    /// <returns><c>false</c>.</returns>
    public bool IsSupported(Type operationType)
    {
        return typeof(ICommand).IsAssignableFrom(operationType) || typeof(IQuery).IsAssignableFrom(operationType);
    }
}
