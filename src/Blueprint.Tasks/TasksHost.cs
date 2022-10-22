using System;
using Blueprint.Configuration;

namespace Blueprint.Tasks;

/// <summary>
/// An <see cref="IBlueprintHost" /> that supports <see cref="IBackgroundTask" />.
/// </summary>
public class TasksHost : IBlueprintHost
{
    /// <inheritdoc />
    public bool IsSupported(Type operationType)
    {
        return typeof(IBackgroundTask).IsAssignableFrom(operationType);
    }
}
