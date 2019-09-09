using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Newtonsoft.Json;
using NLog;
using Blueprint.Core.Utilities;

namespace Blueprint.Core.Tasks
{
    public class TaskScheduler
    {
        private const char IdSplitter = ':';

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly ITaskScheduler[] taskSchedulers;
        private readonly RecurringJobManager recurringJobManager;
        
        public TaskScheduler(ITaskScheduler[] taskSchedulers)
        {
            this.taskSchedulers = taskSchedulers;
            this.recurringJobManager = new RecurringJobManager();
        }

        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public void RescheduleAll()
        {
            var recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            var schedulers = GetSchedulers().ToArray();

            Reschedule(schedulers, recurringJobs);
        }

        [AutomaticRetry(Attempts = 5, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public void StartupCleanup()
        {
            var recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            var schedulers = GetSchedulers().ToArray();

            using (Log.LogTimeWrapper("Purging jobs where the task scheduler no longer exists"))
            {
                var groupNames = schedulers.Select(GetGroupNameFromScheduler).ToList();

                foreach (var recurringJob in recurringJobs)
                {
                    var idParts = recurringJob.Id.Split(IdSplitter);
                    var schedulerName = idParts[0];

                    if (!groupNames.Contains(schedulerName) && recurringJob.Id != "System:Scheduler")
                    {
                        recurringJobManager.RemoveIfExists(recurringJob.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Gets all the schedulers that have been registered.
        /// </summary>
        /// <remarks>
        /// We ask for new schedulers every time to ensure that dependencies that should be transient are (e.g. a
        /// database context should be new every time a scheduler is called)
        /// </remarks>
        /// <returns></returns>
        private IEnumerable<ITaskScheduler> GetSchedulers()
        {
            var schedulerEnabled = "Scheduler.Enabled".GetConfigValue<bool>();

            if (!schedulerEnabled)
            {
                return Enumerable.Empty<ITaskScheduler>();
            }

            return taskSchedulers.Where(s =>
            {
                var enableConfigKey = $"Scheduler.{s.GetType().Name}.Enabled";

                return !enableConfigKey.TryGetAppSetting(out bool isEnabled) || isEnabled;
            });
        }

        private static string GetGroupNameFromScheduler(ITaskScheduler scheduler)
        {
            return scheduler.GetType().Name;
        }

        private void Reschedule(IEnumerable<ITaskScheduler> schedulers, List<RecurringJobDto> recurringJobs)
        {
            Log.Info("Rescheduling tasks");

            foreach (var taskScheduler in schedulers)
            {
                Reschedule(recurringJobs, taskScheduler);
            }
        }

        private void Reschedule(List<RecurringJobDto> existingJobs, ITaskScheduler taskScheduler)
        {
            var schedulerName = taskScheduler.GetType().Name;

            using (Log.LogTimeWrapper("Rescheduling for scheduler={0}", schedulerName))
            {
                var taskSchedules = GetSchedules(taskScheduler, schedulerName).ToList();

                var group = GetGroupNameFromScheduler(taskScheduler);
                var currentNames = taskSchedules.Select(ts => ts.Name).ToList();
                var jobsToDelete = existingJobs.Where(j => j.Id.StartsWith(group))
                    .Where(jk => !currentNames.Contains(jk.Id.Split(IdSplitter)[1]))
                    .ToList();

                Log.Info("Deleting old jobs that no longer exist");

                jobsToDelete.ForEach(j =>
                {
                    Log.Debug("Deleting job that no longer exists. job_to_delete={0}", j.Id);

                    recurringJobManager.RemoveIfExists(j.Id);
                });

                Log.Info("Scheduling (add or update) current jobs");

                // Create and update jobs
                foreach (var taskSchedule in taskSchedules)
                {
                    try
                    {
                        CreateOrUpdateTaskSchedule(existingJobs, taskSchedule, @group);
                    }
                    catch (Exception e)
                    {
                        Log.Error(
                            e,
                            "Could not schedule job='{0}{1}{2}', with schedule='{3}'",
                            @group,
                            IdSplitter,
                            taskSchedule.Name,
                            taskSchedule.CronExpression);
                    }
                }
            }
        }

        private static IEnumerable<TaskSchedule> GetSchedules(ITaskScheduler taskScheduler, string schedulerName)
        {
            try
            {
                Log.Info("Getting scheduled tasks. scheduler={0}", schedulerName);

                return taskScheduler.GetTaskSchedules();

            }
            catch (Exception e)
            {
                Log.Error(
                    e,
                    "Unhandled exception getting schedules from '{0}'. Will delete all existing jobs.",
                    schedulerName);
            }

            return Enumerable.Empty<TaskSchedule>();
        }

        private void CreateOrUpdateTaskSchedule(IEnumerable<RecurringJobDto> existingJobs, TaskSchedule taskSchedule, string group)
        {
            var id = group + IdSplitter + taskSchedule.Name;
            var job = Job.FromExpression<TaskExecutor>(e => e.Execute(taskSchedule.BackgroundTask, null));

            var existing = existingJobs.SingleOrDefault(j => j.Id == id);

            if (existing != null && existing.Cron == taskSchedule.CronExpression && JobsEqual(existing.Job, job))
            {
                Log.Debug("Not rescheduling job as it has not changed. job_id={0}", id);

                return;
            }

            recurringJobManager.AddOrUpdate(
                id,
                job,
                taskSchedule.CronExpression,
                taskSchedule.TimeZone);

            Log.Info("Job scheduled. job_id={0} cron='{1}'", id, taskSchedule.CronExpression);
        }

        private bool JobsEqual(Job existing, Job updated)
        {
            // This job type no longer exists, cannot be equal and must be modified
            if (existing == null)
            {
                return false;
            }

            if (existing.ToString() != updated.ToString())
            {
                return false;
            }

            // We rely on conversion through JSON to check for jobs to see if they are equal
            // or not. This could be made more efficient
            var existingAsJson = JsonConvert.SerializeObject(existing.Args[0]);
            var updatedAsJson = JsonConvert.SerializeObject(updated.Args[0]);

            if (existingAsJson != updatedAsJson)
            {
                return false;
            }

            return true;
        }
    }
}
