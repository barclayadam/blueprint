using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Blueprint.Tasks.Provider;

namespace Blueprint.Tasks
{
    /// <summary>
    /// Represents a "root" scheduled / enqeueued task from <see cref="BackgroundTaskScheduler" />, one that has no parent but supports
    /// continuation through <see cref="IScheduledBackgroundTask.ContinueWith" />.
    /// </summary>
    [DebuggerDisplay("ScheduledBackgroundTask: Type {taskEnvelope.Task.GetType()}")]
    internal class ScheduledBackgroundTask : IScheduledBackgroundTask
    {
        private readonly BackgroundTaskScheduler scheduler;

        private readonly BackgroundTaskEnvelope taskEnvelope;
        private readonly TimeSpan? delay;

        // NB: children is not initialized as in many cases no children will be added so we want to avoid the allocation
        private List<ChildScheduledBackgroundTask> children;

        public ScheduledBackgroundTask(BackgroundTaskEnvelope taskEnvelope, TimeSpan? delay, BackgroundTaskScheduler scheduler)
        {
            this.taskEnvelope = taskEnvelope;
            this.delay = delay;
            this.scheduler = scheduler;
        }

        /// <inheritdoc />
        public IScheduledBackgroundTask ContinueWith(IBackgroundTask backgroundTask, BackgroundTaskContinuationOptions options = BackgroundTaskContinuationOptions.OnlyOnSucceededState)
        {
            if (children == null)
            {
                children = new List<ChildScheduledBackgroundTask>();
            }

            // Copy over the metadata from this parent task, as we know this must have been executed in the
            // same context as this one.
            var backgroundTaskEnvelope = new BackgroundTaskEnvelope(backgroundTask)
            {
                Metadata = taskEnvelope.Metadata,
            };

            var scheduledBackgroundTask = new ChildScheduledBackgroundTask(backgroundTaskEnvelope, options, scheduler);

            children.Add(scheduledBackgroundTask);

            return scheduledBackgroundTask;
        }

        public override string ToString()
        {
            return taskEnvelope.Task.GetType().Name;
        }

        internal async Task PushToProviderAsync(IBackgroundTaskScheduleProvider provider)
        {
            string id;

            if (delay == null)
            {
                id = await provider.EnqueueAsync(taskEnvelope);
            }
            else
            {
                id = await provider.ScheduleAsync(taskEnvelope, delay.Value);
            }

            if (children != null)
            {
                foreach (var child in children)
                {
                    await child.PushToProviderAsync(provider, id);
                }
            }
        }
    }
}
