using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Blueprint.Core;
using Blueprint.Core.Tasks;
using Blueprint.Core.Utilities;

using Hangfire;
using Hangfire.Common;
using Hangfire.States;

using NLog;

using JobContinuationOptions = Blueprint.Core.Tasks.JobContinuationOptions;

namespace Blueprint.Hangfire
{
    public class HangfireBackgroundTaskScheduleProvider : IBackgroundTaskScheduleProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly Lazy<IBackgroundJobClient> jobClientFactory;
        private readonly Dictionary<Type, string> taskToQueue = new Dictionary<Type, string>();

        public HangfireBackgroundTaskScheduleProvider(Lazy<IBackgroundJobClient> jobClientFactory)
        {
            Guard.NotNull(nameof(jobClientFactory), jobClientFactory);

            this.jobClientFactory = jobClientFactory;
        }

        public Task<string> EnqueueAsync(BackgroundTask task)
        {
            return Task.FromResult(CreateCore(task, queue => new EnqueuedState(queue)));
        }

        public Task<string> ScheduleAsync(BackgroundTask task, TimeSpan delay)
        {
            return Task.FromResult(CreateCore(task, queue => new ScheduledState(delay)));
        }

        public Task<string> EnqueueChildAsync(BackgroundTask task, string parentId, JobContinuationOptions options)
        {
            var hangfireOptions = options == JobContinuationOptions.OnlyOnSucceededState
                ? global::Hangfire.JobContinuationOptions.OnlyOnSucceededState
                : global::Hangfire.JobContinuationOptions.OnAnyFinishedState;

            return Task.FromResult(CreateCore(
                task,
                queue => new AwaitingState(parentId, new EnqueuedState(queue), hangfireOptions)));
        }

        private string CreateCore(BackgroundTask task, Func<string, IState> createState)
        {
            var queue = GetQueueForTask(task);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Enqueuing task. task_type={0} queue={1}", task.GetType().Name, queue);
            }

            var id = jobClientFactory.Value.Create(
                Job.FromExpression<TaskExecutor>(e => e.Execute(task, null)),
                createState(queue));

            return id;
        }

        private string GetQueueForTask(BackgroundTask task)
        {
            var taskType = task.GetType();

            if (!taskToQueue.TryGetValue(taskType, out var queue))
            {
                var queueAttribute = taskType.GetAttributesIncludingInterface<QueueAttribute>().SingleOrDefault();
                queue = queueAttribute == null ? EnqueuedState.DefaultQueue : queueAttribute.Queue;

                taskToQueue[taskType] = queue;
            }

            return queue;
        }
    }
}