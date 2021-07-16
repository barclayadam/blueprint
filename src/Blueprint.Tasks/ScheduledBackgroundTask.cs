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
    [DebuggerDisplay("ScheduledBackgroundTask: Type {_taskEnvelope.Task.GetType()}")]
    internal class ScheduledBackgroundTask : IScheduledBackgroundTask
    {
        private readonly BackgroundTaskScheduler _scheduler;

        private readonly BackgroundTaskEnvelope _taskEnvelope;
        private readonly TimeSpan? _delay;

        // NB: children is not initialized as in many cases no children will be added so we want to avoid the allocation
        private List<ChildScheduledBackgroundTask> _children;

        public ScheduledBackgroundTask(BackgroundTaskEnvelope taskEnvelope, TimeSpan? delay, BackgroundTaskScheduler scheduler)
        {
            this._taskEnvelope = taskEnvelope;
            this._delay = delay;
            this._scheduler = scheduler;
        }

        /// <inheritdoc />
        public IScheduledBackgroundTask ContinueWith(IBackgroundTask backgroundTask, BackgroundTaskContinuationOptions options = BackgroundTaskContinuationOptions.OnlyOnSucceededState)
        {
            if (this._children == null)
            {
                this._children = new List<ChildScheduledBackgroundTask>();
            }

            // Copy over the metadata from this parent task, as we know this must have been executed in the
            // same context as this one.
            var backgroundTaskEnvelope = new BackgroundTaskEnvelope(backgroundTask)
            {
                Headers = this._taskEnvelope.Headers,
            };

            var scheduledBackgroundTask = new ChildScheduledBackgroundTask(backgroundTaskEnvelope, options, this._scheduler);

            this._children.Add(scheduledBackgroundTask);

            return scheduledBackgroundTask;
        }

        public override string ToString()
        {
            return this._taskEnvelope.Task.GetType().Name;
        }

        internal async Task PushToProviderAsync(IBackgroundTaskScheduleProvider provider)
        {
            string id;

            if (this._delay == null)
            {
                id = await provider.EnqueueAsync(this._taskEnvelope);
            }
            else
            {
                id = await provider.ScheduleAsync(this._taskEnvelope, this._delay.Value);
            }

            if (this._children != null)
            {
                foreach (var child in this._children)
                {
                    await child.PushToProviderAsync(provider, id);
                }
            }
        }
    }
}
