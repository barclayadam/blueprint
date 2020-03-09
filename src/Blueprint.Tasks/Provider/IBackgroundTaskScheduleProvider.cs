using System;
using System.Threading.Tasks;

namespace Blueprint.Tasks.Provider
{
    /// <summary>
    /// A provider that implements the integration point to actually run a task in the background, once things
    /// like preprocessing and time window handling has occurred.
    /// </summary>
    public interface IBackgroundTaskScheduleProvider
    {
        Task<string> EnqueueAsync(BackgroundTaskEnvelope task);

        Task<string> ScheduleAsync(BackgroundTaskEnvelope task, TimeSpan delay);

        Task<string> EnqueueChildAsync(BackgroundTaskEnvelope task, string parentId, JobContinuationOptions options);
    }
}
