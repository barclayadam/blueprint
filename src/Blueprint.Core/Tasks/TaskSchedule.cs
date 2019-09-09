using System;

namespace Blueprint.Core.Tasks
{
    public class TaskSchedule
    {
        public TaskSchedule(string name, BackgroundTask backgroundTask, string cronExpression)
            : this(name, backgroundTask, cronExpression, null)
        {
        }

        public TaskSchedule(string name, BackgroundTask backgroundTask, string cronExpression, TimeZoneInfo timeZoneInfo)
        {
            Guard.NotNullOrEmpty("name", name);
            Guard.NotNull(nameof(backgroundTask), backgroundTask);
            Guard.NotNullOrEmpty("cronExpression", cronExpression);

            Name = name;
            BackgroundTask = backgroundTask;
            CronExpression = cronExpression;
            TimeZone = timeZoneInfo ?? TimeZoneInfo.Utc;
        }

        public string Name { get; set; }

        public BackgroundTask BackgroundTask { get; protected set; }

        public string CronExpression { get; protected set; }

        public TimeZoneInfo TimeZone { get; protected set; }
    }
}