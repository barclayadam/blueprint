using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        private readonly IRecurringJobManager _recurringJobManager;
        private readonly ILogger<TaskScheduler> _logger;

        /// <summary>
        /// Initialises a new instance of the <see cref="HangfireRecurringTaskProvider" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="recurringJobManager">The Hangfire <see cref="IRecurringJobManager"/>.</param>
        public HangfireRecurringTaskProvider(ILogger<TaskScheduler> logger, IRecurringJobManager recurringJobManager)
        {
            Guard.NotNull(nameof(logger), logger);
            Guard.NotNull(nameof(recurringJobManager), recurringJobManager);

            this._recurringJobManager = recurringJobManager;

            this._logger = logger;
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

            this._logger.LogInformation("Deleting old jobs that no longer exist");

            foreach (var j in jobsToDelete)
            {
                this._recurringJobManager.RemoveIfExists(j.Id);
            }

            this._logger.LogInformation("Scheduling (add or update) current jobs");

            foreach (var kvp in currentAsDictionary)
            {
                var schedule = kvp.Value;

                var envelope = new BackgroundTaskEnvelope(schedule.Schedule.BackgroundTask);

                var job = Job.FromExpression<HangfireTaskExecutor>(
                    e => e.Execute(
                        new HangfireBackgroundTaskWrapper(envelope),
                        null,
                        CancellationToken.None));

                this._recurringJobManager.AddOrUpdate(
                    schedule.Id,
                    job,
                    schedule.Schedule.CronExpression,
                    schedule.Schedule.TimeZone);

                this._logger.LogDebug(
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
            this._recurringJobManager.AddOrUpdate(
                SchedulerJobId,
                Job.FromExpression<RecurringTaskManager>(s => s.RescheduleAllAsync()),
                "*/30 * * * *");

            return Task.CompletedTask;
        }
    }
}
