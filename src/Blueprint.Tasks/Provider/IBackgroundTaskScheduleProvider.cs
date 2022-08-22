using System;
using System.Threading.Tasks;

namespace Blueprint.Tasks.Provider;

/// <summary>
/// A provider that implements the integration point to actually run a task in the background, once things
/// like preprocessing and time window handling has occurred.
/// </summary>
public interface IBackgroundTaskScheduleProvider
{
    /// <summary>
    /// Enqueues the given task for immediate background execution.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <returns>The ID (opaque to clients) that identifies the created task.</returns>
    Task<string> EnqueueAsync(BackgroundTaskEnvelope task);

    /// <summary>
    /// Schedules the given task for execution after a given time period.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <param name="delay">The amount of time to wait before executing the task.</param>
    /// <returns>The ID (opaque to clients) that identifies the created task.</returns>
    Task<string> ScheduleAsync(BackgroundTaskEnvelope task, TimeSpan delay);

    /// <summary>
    /// Enqueues a "child" task, one that depends on the completion of a parent task first.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <param name="parentId">The ID of the parent task this child depends on.</param>
    /// <param name="continuationOptions">Continuation options to determine under what circumstances the
    /// task will be executed.</param>
    /// <returns>The ID (opaque to clients) that identifies the created task.</returns>
    Task<string> EnqueueChildAsync(BackgroundTaskEnvelope task, string parentId, BackgroundTaskContinuationOptions continuationOptions);
}