using System.Collections.Generic;
using System.Reflection;

namespace Blueprint.Core.Tasks
{
    public interface IScheduledTaskRepository
    {
        /// <summary>
        /// Gets the tasks.
        /// </summary>
        /// <param name="assembly">The assembly to load tasks from.</param>
        /// <returns>
        /// All tasks from the named dll that implement ITask, returning the task's full name, CRON expression and the scheduled task, if there is one.
        /// </returns>
        IEnumerable<TaskSchedule> GetTasks(Assembly assembly);
    }
}
