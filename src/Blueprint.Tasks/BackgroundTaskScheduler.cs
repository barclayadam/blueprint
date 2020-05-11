using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Core;
using Blueprint.Core.Apm;
using Blueprint.Core.Tracing;
using Blueprint.Tasks.Provider;
using Microsoft.Extensions.Logging;

namespace Blueprint.Tasks
{
    /// <summary>
    /// The default implementation of <see cref="IBackgroundTaskScheduler" /> that uses a teh
    /// registered <see cref="IBackgroundTaskScheduleProvider" /> to perform the enqueing and
    /// scheduling of <see cref="IBackgroundTask" />s, implementing a unit of work pattern
    /// in which tasks are only "released" once <see cref="IBackgroundTaskScheduler.RunNowAsync" />
    /// is executed.
    /// </summary>
    public class BackgroundTaskScheduler : IBackgroundTaskScheduler
    {
        private readonly ActivityTrackingBackgroundTaskScheduleProvider backgroundTaskScheduleProvider;
        private readonly IEnumerable<IBackgroundTaskPreprocessor> backgroundTaskPreprocessors;
        private readonly IVersionInfoProvider versionInfoProvider;
        private readonly IApmTool apmTool;
        private readonly ILogger<BackgroundTaskScheduler> logger;

        private readonly List<ScheduledBackgroundTask> tasks = new List<ScheduledBackgroundTask>();

        /// <summary>
        /// Initialises a new instance of the <see cref="BackgroundTaskScheduler" /> class.
        /// </summary>
        /// <param name="backgroundTaskPreprocessors">The registered background task preprocessors.</param>
        /// <param name="backgroundTaskScheduleProvider">The provider-specific implementation to delegate to.</param>
        /// <param name="versionInfoProvider">A registered version provider used to decorate tasks with extra metadata.</param>
        /// <param name="apmTool">The registered APM tool used to register a dependency with.</param>
        /// <param name="logger">The logger for this class.</param>
        public BackgroundTaskScheduler(
            IEnumerable<IBackgroundTaskPreprocessor> backgroundTaskPreprocessors,
            IBackgroundTaskScheduleProvider backgroundTaskScheduleProvider,
            IVersionInfoProvider versionInfoProvider,
            IApmTool apmTool,
            ILogger<BackgroundTaskScheduler> logger)
        {
            Guard.NotNull(nameof(backgroundTaskScheduleProvider), backgroundTaskScheduleProvider);
            Guard.NotNull(nameof(versionInfoProvider), versionInfoProvider);
            Guard.NotNull(nameof(apmTool), apmTool);
            Guard.NotNull(nameof(logger), logger);

            this.backgroundTaskScheduleProvider = new ActivityTrackingBackgroundTaskScheduleProvider(backgroundTaskScheduleProvider);
            this.backgroundTaskPreprocessors = backgroundTaskPreprocessors;
            this.versionInfoProvider = versionInfoProvider;
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
        public Task RunNowAsync()
        {
            if (!tasks.Any())
            {
                return Task.CompletedTask;
            }

            return apmTool.TrackDependencyAsync(
                "BackgroundTaskScheduler.RunNowAsync",
                backgroundTaskScheduleProvider.InnerProvider.GetType().Name,
                "Task",
                string.Join(", ", tasks.Select(t => t.ToString())),
                async operation =>
                {
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

                    operation.MarkSuccess();
                });
        }

        private BackgroundTaskEnvelope CreateTaskEnvelope<T>(T task) where T : IBackgroundTask
        {
            var envelope = new BackgroundTaskEnvelope(task)
            {
                Metadata =
                {
                    System = versionInfoProvider.Value.AppName,
                    SystemVersion = versionInfoProvider.Value.Version,
                },
            };

            foreach (var p in backgroundTaskPreprocessors)
            {
                p.Preprocess(envelope);
            }

            return envelope;
        }
    }
}
