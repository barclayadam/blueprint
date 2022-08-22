using Blueprint.Tasks.Provider;

namespace Blueprint.Tasks;

/// <summary>
/// Represents a single <see cref="RecurringTaskSchedule" /> when communicating between the
/// <see cref="RecurringTaskManager" /> and registered <see cref="IRecurringTaskProvider" />.
/// </summary>
public class RecurringTaskScheduleDto
{
    /// <summary>
    /// Initialises a new instance of the <see cref="RecurringTaskScheduleDto" /> class.
    /// </summary>
    /// <param name="id">The unique ID of this schedule.</param>
    /// <param name="schedule">The schedule.</param>
    public RecurringTaskScheduleDto(string id, RecurringTaskSchedule schedule)
    {
        this.Id = id;
        this.Schedule = schedule;
    }

    /// <summary>
    /// The ID of this schedule, which will be unique among all schedules within the system.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The <see cref="RecurringTaskSchedule" />.
    /// </summary>
    public RecurringTaskSchedule Schedule { get; }
}