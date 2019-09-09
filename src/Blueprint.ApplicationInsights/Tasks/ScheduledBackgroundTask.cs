using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Blueprint.Core.Tasks;

namespace Blueprint.ApplicationInsights.Tasks
{
    /// <summary>
    /// Represents a "root" scheduled / enqeueued task from <see cref="BackgroundTaskScheduler" />, one that has no parent but supports
    /// continuation through <see cref="IScheduledBackgroundTask.ContinueWith" />.
    /// </summary>
    [DebuggerDisplay("ScheduledBackgroundTask: Type {task.GetType()}")]
    internal class ScheduledBackgroundTask : IScheduledBackgroundTask
    {
        private readonly BackgroundTaskScheduler scheduler;

        private readonly BackgroundTask task;
        private readonly TimeSpan? delay;

        // NB: children is not initialized as in many cases no children will be added so we want to avoid the allocation
        private List<ChildScheduledBackgroundTask> children;

        public ScheduledBackgroundTask(BackgroundTask task, TimeSpan? delay, BackgroundTaskScheduler scheduler)
        {
            this.task = task;
            this.delay = delay;
            this.scheduler = scheduler;
        }

        /// <inheritdoc />
        public IScheduledBackgroundTask ContinueWith(BackgroundTask backgroundTask, JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
        {
            if (children == null)
            {
                children = new List<ChildScheduledBackgroundTask>();
            }

            // Copy over the metadata from this parent task, as we know this must have been executed in the
            // same context as this one.
            backgroundTask.Metadata = task.Metadata;

            var scheduledBackgroundTask = new ChildScheduledBackgroundTask(backgroundTask, options, scheduler);

            children.Add(scheduledBackgroundTask);

            return scheduledBackgroundTask;
        }

        public bool IsUniqueKeyMatch(Type taskTypeToCheck, string taskToCheckUniqueKey)
        {
            return taskTypeToCheck == task.GetType() && task is IHaveUniqueKey haveUniqueKey && haveUniqueKey.UniqueKey == taskToCheckUniqueKey;
        }

        internal async Task PushToProviderAsync(IBackgroundTaskScheduleProvider provider)
        {
            string id;

            if (delay == null)
            {
                id = await provider.EnqueueAsync(task);
            }
            else
            {
                id = await provider.ScheduleAsync(task, delay.Value);
            }

            if (children != null)
            {
                foreach (var child in children)
                {
                    await child.PushToProviderAsync(provider, id);
                }
            }
        }

        public override string ToString()
        {
            return task.GetType().Name;
        }
    }
}
