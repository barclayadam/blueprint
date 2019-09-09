using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Core.Tasks;

namespace Blueprint.ApplicationInsights.Tasks
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
    internal class ChildScheduledBackgroundTask : IScheduledBackgroundTask
    {
        private readonly BackgroundTask task;
        private readonly JobContinuationOptions option;
        private readonly BackgroundTaskScheduler scheduler;

        // NB: children is not initialized as in many cases no children will be added so we want to avoid the allocation
        private List<ChildScheduledBackgroundTask> children;

        public ChildScheduledBackgroundTask(BackgroundTask task, JobContinuationOptions option, BackgroundTaskScheduler scheduler)
        {
            this.task = task;
            this.option = option;
            this.scheduler = scheduler;
        }

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

        internal async Task PushToProviderAsync(IBackgroundTaskScheduleProvider provider, string parentId)
        {
            var id = await provider.EnqueueChildAsync(task, parentId, option);

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
