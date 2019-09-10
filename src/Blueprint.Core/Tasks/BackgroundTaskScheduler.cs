using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Core.Apm;
using Blueprint.Core.Tracing;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Blueprint.Core.Tasks
{
    public class BackgroundTaskScheduler : IBackgroundTaskScheduler
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        private readonly ActivityTrackingBackgroundTaskScheduleProvider backgroundTaskScheduleProvider;
        private readonly IServiceProvider serviceProvider;
        private readonly IVersionInfoProvider versionInfoProvider;
        private readonly IApmTool apmTool;

        private readonly List<ScheduledBackgroundTask> tasks = new List<ScheduledBackgroundTask>();

        public BackgroundTaskScheduler(
            IServiceProvider serviceProvider,
            IBackgroundTaskScheduleProvider backgroundTaskScheduleProvider,
            IVersionInfoProvider versionInfoProvider,
            IApmTool apmTool)
        {
            Guard.NotNull(nameof(serviceProvider), serviceProvider);
            Guard.NotNull(nameof(backgroundTaskScheduleProvider), backgroundTaskScheduleProvider);
            Guard.NotNull(nameof(versionInfoProvider), versionInfoProvider);
            Guard.NotNull(nameof(apmTool), apmTool);

            this.backgroundTaskScheduleProvider = new ActivityTrackingBackgroundTaskScheduleProvider(backgroundTaskScheduleProvider);
            this.serviceProvider = serviceProvider;
            this.versionInfoProvider = versionInfoProvider;
            this.apmTool = apmTool;
        }

        public Task<IScheduledBackgroundTask> EnqueueAsync<T>(T task) where T : BackgroundTask
        {
            Populate(task);

            var (canQueue, existingTask) = CheckCanEnqueueTask(task);
            if (!canQueue)
            {
                return Task.FromResult((IScheduledBackgroundTask) existingTask);
            }

            var scheduledTask = new ScheduledBackgroundTask(task, null, this);

            tasks.Add(scheduledTask);

            return Task.FromResult((IScheduledBackgroundTask) scheduledTask);
        }

        public Task<IScheduledBackgroundTask> ScheduleAsync<T>(T task, TimeSpan delay) where T : BackgroundTask
        {
            Populate(task);
            
            var (canQueue, existingTask) = CheckCanEnqueueTask(task);
            if (!canQueue)
            {
                return Task.FromResult((IScheduledBackgroundTask) existingTask);
            }

            var scheduledTask = new ScheduledBackgroundTask(task, delay, this);

            tasks.Add(scheduledTask);

            return Task.FromResult((IScheduledBackgroundTask) scheduledTask);
        }

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
                (async operation =>
                {
                    // Clearing tasks before executing so any more calls to execute tasks doesn't re-execute same tasks
                    var currentTasks = new List<ScheduledBackgroundTask>(tasks);

                    tasks.Clear();

                    if (currentTasks.Count > 5)
                    {
                        Log.Warn($"Queuing larger than normal number of tasks: {currentTasks.Count}");
                    }

                    foreach (var task in currentTasks)
                    {
                        await task.PushToProviderAsync(backgroundTaskScheduleProvider);
                    }

                    operation.MarkSuccess();
                }));
        }
        
        private void Populate<T>(T task) where T : BackgroundTask
        {
            task.Metadata.System = versionInfoProvider.Value.AppName;
            task.Metadata.SystemVersion = versionInfoProvider.Value.Version;

            foreach (var p in serviceProvider.GetServices<IBackgroundTaskPreprocessor<T>>())
            {
                p.Preprocess(task);
            }
        }

        private (bool canQueue, ScheduledBackgroundTask existingTask) CheckCanEnqueueTask<T>(T task) where T : BackgroundTask
        {
            if (!(task is IHaveUniqueKey uniqueKeyedTask))
            {
                return (true, null);
            }

            var taskType = task.GetType();

            foreach (var backgroundTask in tasks)
            {
                if (backgroundTask.IsUniqueKeyMatch(taskType, uniqueKeyedTask.UniqueKey))
                {
                    return (false, backgroundTask);
                }
            }

            return (true, null);
        }
    }
}
