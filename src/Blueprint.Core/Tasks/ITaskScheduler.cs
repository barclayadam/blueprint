using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blueprint.Core.Tasks
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public interface ITaskScheduler
    {
        IEnumerable<TaskSchedule> GetTaskSchedules();
    }
}
