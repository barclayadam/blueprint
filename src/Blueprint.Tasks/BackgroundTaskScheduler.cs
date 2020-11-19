using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Apm;
using Blueprint.Tasks.Provider;
using Microsoft.Extensions.Logging;

namespace Blueprint.Tasks
{
    /// <summary>
    /// The default implementation of <see cref="IBackgroundTaskScheduler" /> that uses the
    /// registered <see cref="IBackgroundTaskScheduleProvider" /> to perform the enqueuing and
    /// scheduling of <see cref="IBackgroundTask" />s, implementing a unit of work pattern
    /// in which tasks are only "released" once <see cref="IBackgroundTaskScheduler.RunNowAsync" />
    /// is executed.
    /// </summary>
    public class BackgroundTaskScheduler : IBackgroundTaskScheduler
    {
        private readonly ApmBackgroundTaskScheduleProvider _backgroundTaskScheduleProvider;
        private readonly IEnumerable<IBackgroundTaskPreprocessor> _backgroundTaskPreprocessors;
        private readonly IApmTool _apmTool;
        private readonly ILogger<BackgroundTaskScheduler> _logger;

        private readonly List<ScheduledBackgroundTask> _tasks = new List<ScheduledBackgroundTask>();

        /// <summary>
        /// Initialises a new instance of the <see cref="BackgroundTaskScheduler" /> class.
        /// </summary>
        /// <param name="backgroundTaskPreprocessors">The registered background task preprocessors.</param>
        /// <param name="backgroundTaskScheduleProvider">The provider-specific implementation to delegate to.</param>
        /// <param name="apmTool">The registered APM tool used to register a dependency with.</param>
        /// <param name="logger">The logger for this class.</param>
        public BackgroundTaskScheduler(
            IEnumerable<IBackgroundTaskPreprocessor> backgroundTaskPreprocessors,
            IBackgroundTaskScheduleProvider backgroundTaskScheduleProvider,
            IApmTool apmTool,
            ILogger<BackgroundTaskScheduler> logger)
        {
            Guard.NotNull(nameof(backgroundTaskScheduleProvider), backgroundTaskScheduleProvider);
            Guard.NotNull(nameof(apmTool), apmTool);
            Guard.NotNull(nameof(logger), logger);

            this._backgroundTaskScheduleProvider = new ApmBackgroundTaskScheduleProvider(backgroundTaskScheduleProvider, apmTool);
            this._backgroundTaskPreprocessors = backgroundTaskPreprocessors;
            this._apmTool = apmTool;
            this._logger = logger;
        }

        /// <inheritdoc />
        public IScheduledBackgroundTask Enqueue(IBackgroundTask task)
        {
            var envelope = this.CreateTaskEnvelope(task);
            var scheduledTask = new ScheduledBackgroundTask(envelope, null, this);

            this._tasks.Add(scheduledTask);

            return scheduledTask;
        }

        /// <inheritdoc />
        public IScheduledBackgroundTask Schedule(IBackgroundTask task, TimeSpan delay)
        {
            var envelope = this.CreateTaskEnvelope(task);
            var scheduledTask = new ScheduledBackgroundTask(envelope, delay, this);

            this._tasks.Add(scheduledTask);

            return scheduledTask;
        }

        /// <inheritdoc />
        public async Task RunNowAsync()
        {
            if (!this._tasks.Any())
            {
                return;
            }

            // Clearing tasks before executing so any more calls to execute tasks doesn't re-execute same tasks
            var currentTasks = new List<ScheduledBackgroundTask>(this._tasks);

            this._tasks.Clear();

            if (currentTasks.Count > 5)
            {
                this._logger.LogWarning("Queuing larger than normal number of tasks {0}", currentTasks.Count);
            }

            foreach (var task in currentTasks)
            {
                await task.PushToProviderAsync(this._backgroundTaskScheduleProvider);
            }
        }

        private BackgroundTaskEnvelope CreateTaskEnvelope<T>(T task) where T : IBackgroundTask
        {
            var envelope = new BackgroundTaskEnvelope(task);

            foreach (var p in this._backgroundTaskPreprocessors)
            {
                p.Preprocess(envelope);
            }

            return envelope;
        }
    }
}
