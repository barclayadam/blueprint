namespace Blueprint.Tasks;

/// <summary>
/// An enumeration that indicates how "child" tasks should proceed given the state
/// of it's parent (see <see cref="IScheduledBackgroundTask.ContinueWith" />).
/// </summary>
public enum BackgroundTaskContinuationOptions
{
    /// <summary>
    /// The child task will execute regardless of the state of the parent (i.e. success or
    /// failure).
    /// </summary>
    OnAnyFinishedState,

    /// <summary>
    /// The child will only execute if the parent task completely successfully (including after any number
    /// of failed attempts).
    /// </summary>
    OnlyOnSucceededState,
}