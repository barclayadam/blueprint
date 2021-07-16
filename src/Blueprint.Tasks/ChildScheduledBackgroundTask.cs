using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Blueprint.Tasks.Provider;

namespace Blueprint.Tasks
{
    /// <summary>
    /// A child scheduled background task is one that will be executed after the completion of a "parent" task, one that has
    /// been created from <see cref="IScheduledBackgroundTask.ContinueWith" />.
    /// </summary>
    /// <remarks>
    /// This is similar in scope to <see cref="ScheduledBackgroundTask" /> but expects a parent id when pushing to the
    /// <see cref="IBackgroundTaskScheduleProvider" /> which would have been created from the parent (which would be either another
    /// instance of <see cref="ChildScheduledBackgroundTask" /> or a "root" instance of <see cref="ScheduledBackgroundTask" />.
    /// </remarks>
    [DebuggerDisplay("ChildScheduledBackgroundTask: Type {_taskEnvelope.Task.GetType()}")]
    internal class ChildScheduledBackgroundTask : IScheduledBackgroundTask
    {
        private readonly BackgroundTaskEnvelope _taskEnvelope;
        private readonly BackgroundTaskContinuationOptions _option;
        private readonly BackgroundTaskScheduler _scheduler;

        // NB: children is not initialized as in many cases no children will be added so we want to avoid the allocation
        private List<ChildScheduledBackgroundTask> _children;

        public ChildScheduledBackgroundTask(BackgroundTaskEnvelope taskEnvelope, BackgroundTaskContinuationOptions option, BackgroundTaskScheduler scheduler)
        {
            this._taskEnvelope = taskEnvelope;
            this._option = option;
            this._scheduler = scheduler;
        }

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

        internal async Task PushToProviderAsync(IBackgroundTaskScheduleProvider provider, string parentId)
        {
            var id = await provider.EnqueueChildAsync(this._taskEnvelope, parentId, this._option);

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
