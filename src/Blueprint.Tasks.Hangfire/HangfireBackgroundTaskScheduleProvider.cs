using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blueprint.Diagnostics;
using Blueprint.Tasks.Provider;
using Blueprint.Utilities;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Blueprint.Tasks.Hangfire
{
    /// <summary>
    /// An implementation of <see cref="IBackgroundTaskScheduleProvider" /> that uses Hangfire to
    /// create <see cref="Job" />s that will be executed by <see cref="HangfireTaskExecutor.Execute" />.
    /// </summary>
    public class HangfireBackgroundTaskScheduleProvider : IBackgroundTaskScheduleProvider
    {
        private static readonly Dictionary<Type, string> _taskToQueue = new Dictionary<Type, string>();

        private readonly IBackgroundJobClient _jobClient;
        private readonly ILogger<HangfireBackgroundTaskScheduleProvider> _logger;

        /// <summary>
        /// Initialises a new instance of the <see cref="HangfireBackgroundTaskScheduleProvider" /> class.
        /// </summary>
        /// <param name="jobClient">The registered Hangfire <see cref="IBackgroundJobClient" />.</param>
        /// <param name="logger">The logger.</param>
        public HangfireBackgroundTaskScheduleProvider(IBackgroundJobClient jobClient, ILogger<HangfireBackgroundTaskScheduleProvider> logger)
        {
            Guard.NotNull(nameof(jobClient), jobClient);
            Guard.NotNull(nameof(logger), logger);

            this._jobClient = jobClient;
            this._logger = logger;
        }

        /// <inheritdoc />
        public Task<string> EnqueueAsync(BackgroundTaskEnvelope task)
        {
            return Task.FromResult(this.CreateCore(task, queue => new EnqueuedState(queue)));
        }

        /// <inheritdoc />
        public Task<string> ScheduleAsync(BackgroundTaskEnvelope task, TimeSpan delay)
        {
            return Task.FromResult(this.CreateCore(task, _ => new ScheduledState(delay)));
        }

        /// <inheritdoc />
        public Task<string> EnqueueChildAsync(BackgroundTaskEnvelope task, string parentId, BackgroundTaskContinuationOptions continuationOptions)
        {
            var hangfireOptions = continuationOptions == BackgroundTaskContinuationOptions.OnlyOnSucceededState
                ? JobContinuationOptions.OnlyOnSucceededState
                : JobContinuationOptions.OnAnyFinishedState;

            return Task.FromResult(this.CreateCore(
                task,
                queue => new AwaitingState(parentId, new EnqueuedState(queue), hangfireOptions)));
        }

        private static string GetQueueForTask(IBackgroundTask task)
        {
            var taskType = task.GetType();

            if (!_taskToQueue.TryGetValue(taskType, out var queue))
            {
                var queueAttribute = taskType.GetAttributesIncludingInterface<QueueAttribute>().SingleOrDefault();
                queue = queueAttribute == null ? EnqueuedState.DefaultQueue : queueAttribute.Queue;

                _taskToQueue[taskType] = queue;
            }

            return queue;
        }

        private string CreateCore(BackgroundTaskEnvelope task, Func<string, IState> createState)
        {
            var queue = GetQueueForTask(task.Task);

            if (this._logger.IsEnabled(LogLevel.Debug))
            {
                this._logger.LogDebug("Enqueuing task {TaskType} to {Queue}", task.GetType().Name, queue);
            }

            var hangfireBackgroundTaskEnvelope = new HangfireBackgroundTaskWrapper(task);

            task.Headers = new Dictionary<string, string>();

            using var activity = TaskExecutor.ActivitySource.StartActivity($"{hangfireBackgroundTaskEnvelope.Envelope.Task.GetType()} send", ActivityKind.Producer);

            ActivityContext contextToInject = default;

            if (activity != null)
            {
                contextToInject = activity.Context;
            }
            else if (Activity.Current != null)
            {
                contextToInject = Activity.Current.Context;
            }

            TaskExecutor.Propagator.Inject(
                new PropagationContext(contextToInject, Baggage.Current),
                task.Headers,
                (c, k, v) => c[k] = v);

            activity?.SetTag("messaging.system", "hangfire");
            activity?.SetTag("messaging.destination", queue);
            activity?.SetTag("messaging.destination_kind", "queue");

            var id = this._jobClient.Create(
                Job.FromExpression<HangfireTaskExecutor>(e => e.Execute(
                        hangfireBackgroundTaskEnvelope,
                        null,
                        CancellationToken.None)),
                createState(queue));

            return id;
        }
    }
}
