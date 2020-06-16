using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blueprint.Core;
using Blueprint.Tasks.Provider;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;

namespace Blueprint.Tasks.Hangfire
{
    /// <summary>
    /// An <see cref="IRecurringTaskProvider" /> that integrates in to Hangfire.
    /// </summary>
    public class HangfireRecurringTaskProvider : IRecurringTaskProvider
    {
        private const string SchedulerJobId = "System:Scheduler";

        private readonly IRecurringJobManager recurringJobManager;
        private readonly ILogger<TaskScheduler> logger;

        /// <summary>
        /// Initialises a new instance of the <see cref="HangfireRecurringTaskProvider" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="recurringJobManager">The Hangfire <see cref="IRecurringJobManager"/>.</param>
        public HangfireRecurringTaskProvider(ILogger<TaskScheduler> logger, IRecurringJobManager recurringJobManager)
        {
            Guard.NotNull(nameof(logger), logger);
            Guard.NotNull(nameof(recurringJobManager), recurringJobManager);

            this.recurringJobManager = recurringJobManager;

            this.logger = logger;
        }

        /// <inheritdoc />
        public Task UpdateAsync(IEnumerable<RecurringTaskScheduleDto> current)
        {
            var existingJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            var currentAsDictionary = current.ToDictionary(k => k.Id, v => v);

            // Finds jobs that exist in Hangfire but _do not_ exist in current list (with special case of
            // the system scheduler)
            var jobsToDelete = existingJobs
                .Where(jk => !currentAsDictionary.ContainsKey(jk.Id) && jk.Id != SchedulerJobId)
                .ToList();

            logger.LogInformation("Deleting old jobs that no longer exist");

            foreach (var j in jobsToDelete)
            {
                recurringJobManager.RemoveIfExists(j.Id);
            }

            logger.LogInformation("Scheduling (add or update) current jobs");

            foreach (var kvp in currentAsDictionary)
            {
                var schedule = kvp.Value;

                var envelope = new BackgroundTaskEnvelope(schedule.Schedule.BackgroundTask);

                var job = Job.FromExpression<HangfireTaskExecutor>(
                    e => e.Execute(
                        new HangfireBackgroundTaskWrapper(envelope),
                        null,
                        CancellationToken.None));

                recurringJobManager.AddOrUpdate(
                    schedule.Id,
                    job,
                    schedule.Schedule.CronExpression,
                    schedule.Schedule.TimeZone);

                logger.LogDebug(
                    "Scheduled job {JobName} ({JobId}) with cron {CronExpression}",
                    schedule.Schedule.Name,
                    schedule.Id,
                    schedule.Schedule.CronExpression);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetupRecurringManagerAsync()
        {
            recurringJobManager.AddOrUpdate(
                SchedulerJobId,
                Job.FromExpression<RecurringTaskManager>(s => s.RescheduleAllAsync()),
                "*/30 * * * *");

            return Task.CompletedTask;
        }
    }
}
