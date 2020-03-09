using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blueprint.Tasks
{
    /// <summary>
    /// Extension methods for <see cref="IBackgroundTaskScheduler" />.
    /// </summary>
    public static class BackgroundTaskSchedulerExtensions
    {
        /// <summary>
        /// Executes a number of task in a sequential fashion, one after the other.
        /// </summary>
        /// <param name="backgroundTaskScheduler">The scheduler on which to enqueue tasks.</param>
        /// <param name="tasks">The tasks that should be scheduled.</param>
        /// <param name="options">Options used to determine in what state the task should be considered for execution.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning the <strong>final</strong> scheduled background task.</returns>
        public static async Task<IScheduledBackgroundTask> EnqueueSequentiallyAsync(
            this IBackgroundTaskScheduler backgroundTaskScheduler,
            IEnumerable<IBackgroundTask> tasks,
            BackgroundTaskContinuationOptions options = BackgroundTaskContinuationOptions.OnlyOnSucceededState)
        {
            IScheduledBackgroundTask scheduledTask = null;

            foreach (var taskToRun in tasks)
            {
                scheduledTask = scheduledTask == null ?
                                    await backgroundTaskScheduler.EnqueueAsync(taskToRun) :
                                    scheduledTask.ContinueWith(taskToRun, options);
            }

            return scheduledTask;
        }
    }
}
