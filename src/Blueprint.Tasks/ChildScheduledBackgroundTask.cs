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
    [DebuggerDisplay("ChildScheduledBackgroundTask: Type {taskEnvelope.BackgroundTask.GetType()}")]
    internal class ChildScheduledBackgroundTask : IScheduledBackgroundTask
    {
        private readonly BackgroundTaskEnvelope taskEnvelope;
        private readonly JobContinuationOptions option;
        private readonly BackgroundTaskScheduler scheduler;

        // NB: children is not initialized as in many cases no children will be added so we want to avoid the allocation
        private List<ChildScheduledBackgroundTask> children;

        public ChildScheduledBackgroundTask(BackgroundTaskEnvelope taskEnvelope, JobContinuationOptions option, BackgroundTaskScheduler scheduler)
        {
            this.taskEnvelope = taskEnvelope;
            this.option = option;
            this.scheduler = scheduler;
        }

        public IScheduledBackgroundTask ContinueWith(IBackgroundTask backgroundTask, JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
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

        internal async Task PushToProviderAsync(IBackgroundTaskScheduleProvider provider, string parentId)
        {
            var id = await provider.EnqueueChildAsync(taskEnvelope, parentId, option);

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
