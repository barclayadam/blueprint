﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Core;
using Blueprint.Core.Apm;
using Blueprint.Core.Tracing;
using Blueprint.Tasks.Provider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blueprint.Tasks
{
    public class BackgroundTaskScheduler : IBackgroundTaskScheduler
    {
        private readonly ActivityTrackingBackgroundTaskScheduleProvider backgroundTaskScheduleProvider;
        private readonly IServiceProvider serviceProvider;
        private readonly IVersionInfoProvider versionInfoProvider;
        private readonly IApmTool apmTool;
        private readonly ILogger<BackgroundTaskScheduler> logger;

        private readonly List<ScheduledBackgroundTask> tasks = new List<ScheduledBackgroundTask>();

        public BackgroundTaskScheduler(
            IServiceProvider serviceProvider,
            IBackgroundTaskScheduleProvider backgroundTaskScheduleProvider,
            IVersionInfoProvider versionInfoProvider,
            IApmTool apmTool,
            ILogger<BackgroundTaskScheduler> logger)
        {
            Guard.NotNull(nameof(serviceProvider), serviceProvider);
            Guard.NotNull(nameof(backgroundTaskScheduleProvider), backgroundTaskScheduleProvider);
            Guard.NotNull(nameof(versionInfoProvider), versionInfoProvider);
            Guard.NotNull(nameof(apmTool), apmTool);
            Guard.NotNull(nameof(logger), logger);

            this.backgroundTaskScheduleProvider = new ActivityTrackingBackgroundTaskScheduleProvider(backgroundTaskScheduleProvider);
            this.serviceProvider = serviceProvider;
            this.versionInfoProvider = versionInfoProvider;
            this.apmTool = apmTool;
            this.logger = logger;
        }

        /// <inheritdoc />
        public Task<IScheduledBackgroundTask> EnqueueAsync<T>(T task) where T : IBackgroundTask
        {
            var (canQueue, existingTask) = CheckCanEnqueueTask(task);
            if (!canQueue)
            {
                return Task.FromResult((IScheduledBackgroundTask)existingTask);
            }

            var envelope = CreateTaskEnvelope(task);
            var scheduledTask = new ScheduledBackgroundTask(envelope, null, this);

            tasks.Add(scheduledTask);

            return Task.FromResult((IScheduledBackgroundTask)scheduledTask);
        }

        /// <inheritdoc />
        public Task<IScheduledBackgroundTask> ScheduleAsync<T>(T task, TimeSpan delay) where T : IBackgroundTask
        {

            var (canQueue, existingTask) = CheckCanEnqueueTask(task);

            if (!canQueue)
            {
                return Task.FromResult((IScheduledBackgroundTask)existingTask);
            }

            var envelope = CreateTaskEnvelope(task);
            var scheduledTask = new ScheduledBackgroundTask(envelope, delay, this);

            tasks.Add(scheduledTask);

            return Task.FromResult((IScheduledBackgroundTask)scheduledTask);
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

            foreach (var p in serviceProvider.GetServices<IBackgroundTaskPreprocessor<T>>())
            {
                p.Preprocess(task);
            }

            return envelope;
        }

        private (bool canQueue, ScheduledBackgroundTask existingTask) CheckCanEnqueueTask<T>(T task) where T : IBackgroundTask
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
