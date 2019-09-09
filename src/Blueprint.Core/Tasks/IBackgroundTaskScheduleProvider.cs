using System;
using System.Threading.Tasks;

namespace Blueprint.Core.Tasks
{
    /// <summary>
    /// A provider that implements the integration point to actually run a task in the background, once things
    /// like preprocessing and time window handling has occurred.
    /// </summary>
    public interface IBackgroundTaskScheduleProvider
    {
        Task<string> EnqueueAsync(BackgroundTask task);

        Task<string> ScheduleAsync(BackgroundTask task, TimeSpan delay);

        Task<string> EnqueueChildAsync(BackgroundTask task, string parentId, JobContinuationOptions options);
    }
}