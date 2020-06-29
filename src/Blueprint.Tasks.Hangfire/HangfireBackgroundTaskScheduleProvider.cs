using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Blueprint;
using Blueprint.Utilities;
using Blueprint.Tasks;
using Blueprint.Tasks.Provider;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging;

namespace Blueprint.Tasks.Hangfire
{
    /// <summary>
    /// An implementation of <see cref="IBackgroundTaskScheduleProvider" /> that uses Hangfire to
    /// create <see cref="Job" />s that will be executed by <see cref="HangfireTaskExecutor.Execute" />.
    /// </summary>
    public class HangfireBackgroundTaskScheduleProvider : IBackgroundTaskScheduleProvider
    {
        private readonly IBackgroundJobClient jobClient;
        private readonly ILogger<HangfireBackgroundTaskScheduleProvider> logger;
        private readonly Dictionary<Type, string> taskToQueue = new Dictionary<Type, string>();

        /// <summary>
        /// Initialises a new instance of the <see cref="HangfireBackgroundTaskScheduleProvider" /> class.
        /// </summary>
        /// <param name="jobClient">The registered Hangfire <see cref="IBackgroundJobClient" />.</param>
        /// <param name="logger">The logger.</param>
        public HangfireBackgroundTaskScheduleProvider(IBackgroundJobClient jobClient, ILogger<HangfireBackgroundTaskScheduleProvider> logger)
        {
            Guard.NotNull(nameof(jobClient), jobClient);
            Guard.NotNull(nameof(logger), logger);

            this.jobClient = jobClient;
            this.logger = logger;
        }

        /// <inheritdoc />
        public Task<string> EnqueueAsync(BackgroundTaskEnvelope task)
        {
            return Task.FromResult(CreateCore(task, queue => new EnqueuedState(queue)));
        }

        /// <inheritdoc />
        public Task<string> ScheduleAsync(BackgroundTaskEnvelope task, TimeSpan delay)
        {
            return Task.FromResult(CreateCore(task, queue => new ScheduledState(delay)));
        }

        /// <inheritdoc />
        public Task<string> EnqueueChildAsync(BackgroundTaskEnvelope task, string parentId, BackgroundTaskContinuationOptions continuationOptions)
        {
            var hangfireOptions = continuationOptions == BackgroundTaskContinuationOptions.OnlyOnSucceededState
                ? JobContinuationOptions.OnlyOnSucceededState
                : JobContinuationOptions.OnAnyFinishedState;

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
                Job.FromExpression<HangfireTaskExecutor>(e => e.Execute(
                    new HangfireBackgroundTaskWrapper(task),
                    null,
                    CancellationToken.None)),
                createState(queue));

            return id;
        }

        private string GetQueueForTask(BackgroundTaskEnvelope envelope)
        {
            var taskType = envelope.Task.GetType();

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
