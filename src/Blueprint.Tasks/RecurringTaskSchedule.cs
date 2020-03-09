using System;
using Blueprint.Core;

namespace Blueprint.Tasks
{
    /// <summary>
    /// Represents a single recurring schedule for a <see cref="IBackgroundTask" />, a task that
    /// on a given time cadence will be executed by the background task processor.
    /// </summary>
    public class RecurringTaskSchedule
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="RecurringTaskSchedule" /> class in the <see cref="TimeZoneInfo.Utc" />
        /// time zone.
        /// </summary>
        /// <param name="name">The (unique) name of this schedule.</param>
        /// <param name="backgroundTask">The task that should be executed.</param>
        /// <param name="cronExpression">A CRON expression representing when the task should be executed.</param>
        public RecurringTaskSchedule(string name, IBackgroundTask backgroundTask, string cronExpression)
            : this(name, backgroundTask, cronExpression, null)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="RecurringTaskSchedule" /> class in the <see cref="TimeZoneInfo.Utc" />
        /// time zone.
        /// </summary>
        /// <param name="name">The (unique) name of this schedule.</param>
        /// <param name="backgroundTask">The task that should be executed.</param>
        /// <param name="cronExpression">A CRON expression representing when the task should be executed.</param>
        /// <param name="timeZoneInfo">The time zone the CRON expression is in.</param>
        public RecurringTaskSchedule(string name, IBackgroundTask backgroundTask, string cronExpression, TimeZoneInfo timeZoneInfo)
        {
            Guard.NotNullOrEmpty(nameof(name), name);
            Guard.NotNull(nameof(backgroundTask), backgroundTask);
            Guard.NotNullOrEmpty(nameof(cronExpression), cronExpression);

            Name = name;
            BackgroundTask = backgroundTask;
            CronExpression = cronExpression;
            TimeZone = timeZoneInfo ?? TimeZoneInfo.Utc;
        }

        /// <summary>
        /// The unique name of this task.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The task to execute.
        /// </summary>
        public IBackgroundTask BackgroundTask { get; protected set; }

        /// <summary>
        /// The CRON expression describing the schedule.
        /// </summary>
        public string CronExpression { get; protected set; }

        /// <summary>
        /// The time zone the CRON expression will be interpreted for.
        /// </summary>
        public TimeZoneInfo TimeZone { get; protected set; }
    }
}
