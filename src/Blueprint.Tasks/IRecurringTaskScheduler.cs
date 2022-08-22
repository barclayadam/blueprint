using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Blueprint.Tasks;

/// <summary>
/// A recurring task scheduler is responsible for creating 0 or more <see cref="RecurringTaskSchedule" />
/// that represent <see cref="IBackgroundTask" />s that should be executed on a regular basis.
/// </summary>
/// <remarks>
/// Each registered scheduler will be executed on a regular cadence to discover the desired set of schedules, meaning
/// the return value should be whatever is current at the point of execution.
/// </remarks>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public interface IRecurringTaskScheduler
{
    /// <summary>
    /// Gets the task schedules to register.
    /// </summary>
    /// <returns>The task schedules to register.</returns>
    Task<IEnumerable<RecurringTaskSchedule>> GetTaskSchedulesAsync();
}