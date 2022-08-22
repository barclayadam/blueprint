using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blueprint.Tasks.Provider;

/// <summary>
/// A provider that allows scheduling <see cref="RecurringTaskSchedule" />s to be run in the background
/// using a <see cref="TaskExecutor" />.
/// </summary>
public interface IRecurringTaskProvider
{
    /// <summary>
    /// Given the set of current <see cref="RecurringTaskScheduleDto" /> updates the state of this
    /// provider to match, deleting old ones, updating existing and creating new ones.
    /// </summary>
    /// <param name="current">The current set of schedules.</param>
    /// <returns>A <see cref="Task"/> representing this operation.</returns>
    Task UpdateAsync(IEnumerable<RecurringTaskScheduleDto> current);

    /// <summary>
    /// Starts a task that will execute <see cref="RecurringTaskManager.RescheduleAsync" /> at a regular
    /// cadence.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing this operation.</returns>
    Task SetupRecurringManagerAsync();
}