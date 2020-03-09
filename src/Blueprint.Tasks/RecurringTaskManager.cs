﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Core;
using Blueprint.Core.Utilities;
using Blueprint.Tasks.Provider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blueprint.Tasks
{
    /// <summary>
    /// The <see cref="RecurringTaskManager" /> is responsible for interacting with an external task manager
    /// and the set of registered <see cref="IRecurringTaskScheduler" />s to create a number of scheduled
    /// <see cref="IBackgroundTask" />s.
    /// </summary>
    public class RecurringTaskManager
    {
        private const char IdSplitter = ':';

        private readonly IRecurringTaskScheduler[] taskSchedulers;
        private readonly IRecurringTaskProvider provider;
        private readonly ILogger<TaskScheduler> logger;
        private readonly IOptions<RecurringTaskManagerOptions> options;

        /// <summary>
        /// Initialises a new instance of the <see cref="RecurringTaskManager" /> class.
        /// </summary>
        /// <param name="taskSchedulers">The recurring task schedulers that will be used to create the
        /// schedules.</param>
        /// <param name="provider">The provider that implements the actual scheduling.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The task options.</param>
        public RecurringTaskManager(
            IRecurringTaskScheduler[] taskSchedulers,
            IRecurringTaskProvider provider,
            ILogger<TaskScheduler> logger,
            IOptions<RecurringTaskManagerOptions> options)
        {
            Guard.NotNull(nameof(taskSchedulers), taskSchedulers);
            Guard.NotNull(nameof(provider), provider);
            Guard.NotNull(nameof(logger), logger);
            Guard.NotNull(nameof(options), options);

            this.taskSchedulers = taskSchedulers;
            this.provider = provider;
            this.logger = logger;
            this.options = options;
        }

        /// <summary>
        /// Reschedules all recurring tasks and ensured the provider has the _current_ set of
        /// <see cref="RecurringTaskSchedule"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RescheduleAllAsync()
        {
            var schedulers = GetActiveSchedulers().ToArray();
            var current = new List<RecurringTaskScheduleDto>();

            logger.LogInformation("Rescheduling tasks from {SchedulerCount} schedulers", schedulers.Length);

            foreach (var taskScheduler in schedulers)
            {
                Reschedule(taskScheduler, current);
            }

            await provider.UpdateAsync(current);
        }

        private static string GetGroupNameFromScheduler(IRecurringTaskScheduler scheduler)
        {
            return scheduler.GetType().Name;
        }

        private IEnumerable<RecurringTaskSchedule> GetSchedules(IRecurringTaskScheduler recurringTaskScheduler, string schedulerName)
        {
            try
            {
                logger.LogInformation("Getting scheduled tasks. scheduler={0}", schedulerName);

                return recurringTaskScheduler.GetTaskSchedules();
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    "Unhandled exception getting schedules from '{0}'. Will delete all existing jobs.",
                    schedulerName);
            }

            return Enumerable.Empty<RecurringTaskSchedule>();
        }

        /// <summary>
        /// Gets all the schedulers that have been registered and are currently active.
        /// </summary>
        /// <remarks>
        /// We ask for new schedulers every time to ensure that dependencies that should be transient are (e.g. a
        /// database context should be new every time a scheduler is called).
        /// </remarks>
        /// <returns>A list of active schedulers.</returns>
        private IEnumerable<IRecurringTaskScheduler> GetActiveSchedulers()
        {
            var loadedOptions = options.Value;
            var schedulerEnabled = loadedOptions.SchedulerEnabled;

            if (!schedulerEnabled)
            {
                return Enumerable.Empty<IRecurringTaskScheduler>();
            }

            return taskSchedulers.Where(s => !loadedOptions.DisabledSchedulers.Contains(s.GetType().Name));
        }

        private void Reschedule(IRecurringTaskScheduler recurringTaskScheduler, List<RecurringTaskScheduleDto> current)
        {
            var schedulerName = recurringTaskScheduler.GetType().Name;

            using (logger.LogTimeWrapper("Getting schedules for scheduler {SchedulerName}", schedulerName))
            {
                var taskSchedules = GetSchedules(recurringTaskScheduler, schedulerName).ToList();

                var group = GetGroupNameFromScheduler(recurringTaskScheduler);

                // Create and update jobs
                foreach (var taskSchedule in taskSchedules)
                {
                    var id = group + IdSplitter + taskSchedule.Name;

                    current.Add(new RecurringTaskScheduleDto(id, taskSchedule));
                }
            }
        }
    }
}
