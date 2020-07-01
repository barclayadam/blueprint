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
        private readonly ApmBackgroundTaskScheduleProvider backgroundTaskScheduleProvider;
        private readonly IEnumerable<IBackgroundTaskPreprocessor> backgroundTaskPreprocessors;
        private readonly IApmTool apmTool;
        private readonly ILogger<BackgroundTaskScheduler> logger;

        private readonly List<ScheduledBackgroundTask> tasks = new List<ScheduledBackgroundTask>();

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

            this.backgroundTaskScheduleProvider = new ApmBackgroundTaskScheduleProvider(backgroundTaskScheduleProvider, apmTool);
            this.backgroundTaskPreprocessors = backgroundTaskPreprocessors;
            this.apmTool = apmTool;
            this.logger = logger;
        }

        /// <inheritdoc />
        public IScheduledBackgroundTask Enqueue<T>(T task) where T : IBackgroundTask
        {
            var envelope = CreateTaskEnvelope(task);
            var scheduledTask = new ScheduledBackgroundTask(envelope, null, this);

            tasks.Add(scheduledTask);

            return scheduledTask;
        }

        /// <inheritdoc />
        public IScheduledBackgroundTask Schedule<T>(T task, TimeSpan delay) where T : IBackgroundTask
        {
            var envelope = CreateTaskEnvelope(task);
            var scheduledTask = new ScheduledBackgroundTask(envelope, delay, this);

            tasks.Add(scheduledTask);

            return scheduledTask;
        }

        /// <inheritdoc />
        public async Task RunNowAsync()
        {
            if (!tasks.Any())
            {
                return;
            }

            // Clearing tasks before executing so any more calls to execute tasks doesn't re-execute same tasks
            var currentTasks = new List<ScheduledBackgroundTask>(tasks);

            tasks.Clear();

            if (currentTasks.Count > 5)
            {
                logger.LogWarning("Queuing larger than normal number of tasks {0}", currentTasks.Count);
            }

            foreach (var task in currentTasks)
            {
                await task.PushToProviderAsync(backgroundTaskScheduleProvider);
            }
        }

        private BackgroundTaskEnvelope CreateTaskEnvelope<T>(T task) where T : IBackgroundTask
        {
            var envelope = new BackgroundTaskEnvelope(task);

            foreach (var p in backgroundTaskPreprocessors)
            {
                p.Preprocess(envelope);
            }

            return envelope;
        }
    }
}
