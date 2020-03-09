using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blueprint.Tasks
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public interface IRecurringTaskScheduler
    {
        IEnumerable<RecurringTaskSchedule> GetTaskSchedules();
    }
}
