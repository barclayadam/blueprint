namespace Blueprint.Tasks;

/// <summary>
/// Represents a scheduled background task.
/// </summary>
public interface IScheduledBackgroundTask
{
    /// <summary>
    /// Adds a new task that will execute *after* this task, with options to determine whether it should
    /// run onl on successful execution (the default) or not.
    /// </summary>
    /// <param name="backgroundTask">The task to execute.</param>
    /// <param name="options">Options used to determine in what state the task should be considered for execution.</param>
    /// <returns>A new scheduled background task.</returns>
    IScheduledBackgroundTask ContinueWith(IBackgroundTask backgroundTask, BackgroundTaskContinuationOptions options = BackgroundTaskContinuationOptions.OnlyOnSucceededState);
}