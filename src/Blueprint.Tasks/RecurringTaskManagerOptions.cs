using System;

namespace Blueprint.Tasks;

/// <summary>
/// The options for <see cref="RecurringTaskManager" />.
/// </summary>
public class RecurringTaskManagerOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the scheduled is enabled at all.
    /// </summary>
    public bool SchedulerEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets an array of type names of schedulers that should be disabled.
    /// </summary>
    public string[] DisabledSchedulers { get; set; } = Array.Empty<string>();
}