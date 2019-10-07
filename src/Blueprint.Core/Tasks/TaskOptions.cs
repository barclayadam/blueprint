namespace Blueprint.Core.Tasks
{
    /// <summary>
    /// The options for <see cref="TaskScheduler" />.
    /// </summary>
    public class TaskOptions
    {
        /// <summary>
        /// Gets or sets an array of type names of tasks that should be disabled.
        /// </summary>
        public string[] DisabledTasks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the scheduled is enabled at all.
        /// </summary>
        public bool SchedulerEnabled { get; set; }

        /// <summary>
        /// Gets or sets an array of type names of schedulers that should be disabled.
        /// </summary>
        public string[] DisabledSchedulers { get; set; }
    }
}
