using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Core;
using Blueprint.Core.Utilities;
using Blueprint.Tasks;
using Blueprint.Tasks.Provider;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;

namespace Blueprint.Hangfire
{
    /// <summary>
    /// An <see cref="IRecurringTaskProvider" /> that integrates in to Hangfire.
    /// </summary>
    public class HangfireRecurringTaskProvider : IRecurringTaskProvider
    {
        private readonly RecurringJobManager recurringJobManager;
        private readonly ILogger<TaskScheduler> logger;

        /// <summary>
        /// Initialises a new instance of the <see cref="HangfireRecurringTaskProvider" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public HangfireRecurringTaskProvider(ILogger<TaskScheduler> logger)
        {
            Guard.NotNull(nameof(logger), logger);

            recurringJobManager = new RecurringJobManager();

            this.logger = logger;
        }

        /// <inheritdoc />
        public Task UpdateAsync(IEnumerable<RecurringTaskScheduleDto> current)
        {
            var existingJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            var toDictionary = existingJobs.ToDictionary(k => k.Id, v => 1);

            var jobsToDelete = existingJobs
                .Where(jk => toDictionary.ContainsKey(jk.Id))
                .ToList();

            using (logger.LogTimeWrapper("Deleting old jobs that no longer exist"))
            {
                foreach (var j in jobsToDelete)
                {
                    recurringJobManager.RemoveIfExists(j.Id);
                }
            }

            using (logger.LogTimeWrapper("Scheduling (add or update) current jobs"))
            {
                foreach (var r in current)
                {
                    var envelope = new BackgroundTaskEnvelope(r.Schedule.BackgroundTask)
                    {
                        Metadata =
                        {
                            System = nameof(HangfireRecurringTaskProvider),
                        },
                    };

                    var job = Job.FromExpression<HangfireTaskExecutor>(e => e.Execute(new HangfireBackgroundTaskWrapper(envelope), null));

                    recurringJobManager.AddOrUpdate(
                        r.Id,
                        job,
                        r.Schedule.CronExpression,
                        r.Schedule.TimeZone);

                    logger.LogDebug(
                        "Scheduled job {JobName} ({JobId}) with cron {CronExpression}",
                        r.Schedule.Name,
                        r.Id,
                        r.Schedule.CronExpression);
                }
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetupRecurringManagerAsync()
        {
            recurringJobManager.AddOrUpdate(
                "System:Scheduler",
                Job.FromExpression<RecurringTaskManager>(s => s.RescheduleAllAsync()), "*/30 * * * *");

            return Task.CompletedTask;
        }
    }
}
