using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Blueprint.Core;
using Blueprint.Core.Utilities;
using Blueprint.Tasks;
using Blueprint.Tasks.Provider;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using JobContinuationOptions = Blueprint.Tasks.JobContinuationOptions;

namespace Blueprint.Hangfire
{
    public class HangfireBackgroundTaskScheduleProvider : IBackgroundTaskScheduleProvider
    {
        private readonly IBackgroundJobClient jobClient;
        private readonly ILogger<HangfireBackgroundTaskScheduleProvider> logger;
        private readonly Dictionary<Type, string> taskToQueue = new Dictionary<Type, string>();

        public HangfireBackgroundTaskScheduleProvider(IBackgroundJobClient jobClient, ILogger<HangfireBackgroundTaskScheduleProvider> logger)
        {
            Guard.NotNull(nameof(jobClient), jobClient);
            Guard.NotNull(nameof(logger), logger);

            this.jobClient = jobClient;
            this.logger = logger;
        }

        public Task<string> EnqueueAsync(BackgroundTaskEnvelope task)
        {
            return Task.FromResult(CreateCore(task, queue => new EnqueuedState(queue)));
        }

        public Task<string> ScheduleAsync(BackgroundTaskEnvelope task, TimeSpan delay)
        {
            return Task.FromResult(CreateCore(task, queue => new ScheduledState(delay)));
        }

        public Task<string> EnqueueChildAsync(BackgroundTaskEnvelope task, string parentId, JobContinuationOptions options)
        {
            var hangfireOptions = options == JobContinuationOptions.OnlyOnSucceededState
                ? global::Hangfire.JobContinuationOptions.OnlyOnSucceededState
                : global::Hangfire.JobContinuationOptions.OnAnyFinishedState;

            return Task.FromResult(CreateCore(
                task,
                queue => new AwaitingState(parentId, new EnqueuedState(queue), hangfireOptions)));
        }

        private string CreateCore(BackgroundTaskEnvelope task, Func<string, IState> createState)
        {
            var queue = GetQueueForTask(task);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Enqueuing task. task_type={0} queue={1}", task.GetType().Name, queue);
            }

            var id = jobClient.Create(
                Job.FromExpression<HangfireTaskExecutor>(e => e.Execute(new HangfireBackgroundTaskWrapper(task), null)),
                createState(queue));

            return id;
        }

        private string GetQueueForTask(BackgroundTaskEnvelope envelope)
        {
            var taskType = envelope.BackgroundTask.GetType();

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
