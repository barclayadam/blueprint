using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Blueprint.Apm;
using Blueprint.Tasks.Provider;

namespace Blueprint.Tasks
{
    /// <summary>
    /// A wrapper for a <see cref="IBackgroundTaskScheduleProvider" /> implementation that will use the
    /// registered <see cref="IApmTool" /> to integrate with distributed tracing mechanisms for monitoring
    /// tasks.
    /// </summary>
    public class ApmBackgroundTaskScheduleProvider : IBackgroundTaskScheduleProvider
    {
        private readonly IBackgroundTaskScheduleProvider _innerProvider;
        private readonly IApmTool _apmTool;

        /// <summary>
        /// Initialises a new instance of the <see cref="ApmBackgroundTaskScheduleProvider" /> class.
        /// </summary>
        /// <param name="innerProvider">The <see cref="IBackgroundTaskScheduleProvider" /> to wrap.</param>
        /// <param name="apmTool">The registered APM tool to enable cross-process tracing.</param>
        public ApmBackgroundTaskScheduleProvider(IBackgroundTaskScheduleProvider innerProvider, IApmTool apmTool)
        {
            this._innerProvider = innerProvider;
            this._apmTool = apmTool;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method will set the request ID and baggage from the current ambient <see cref="Activity" /> on to the
        /// background task.
        /// </remarks>
        public Task<string> EnqueueAsync(BackgroundTaskEnvelope task)
        {
            return this.TrackAsync(task, () => this._innerProvider.EnqueueAsync(task));
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method will set the request ID and baggage from the current ambient <see cref="Activity" /> on to the
        /// background task.
        /// </remarks>
        public Task<string> ScheduleAsync(BackgroundTaskEnvelope task, TimeSpan delay)
        {
            return this.TrackAsync(task, () => this._innerProvider.ScheduleAsync(task, delay));
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method will set the request ID and baggage from the current ambient <see cref="Activity" /> on to the
        /// background task.
        /// </remarks>
        public Task<string> EnqueueChildAsync(BackgroundTaskEnvelope task, string parentId, BackgroundTaskContinuationOptions continuationOptions)
        {
            return this.TrackAsync(task, () => this._innerProvider.EnqueueChildAsync(task, parentId, continuationOptions));
        }

        private async Task<string> TrackAsync(BackgroundTaskEnvelope task, Func<Task<string>> fn)
        {
            task.ApmContext = new Dictionary<string, string>();

            using var op = this._apmTool.Start(
                SpanKinds.Producer,
                "task.enqueue",
                "queue");

            op.InjectContext(task.ApmContext);

            return await fn();
        }
    }
}
