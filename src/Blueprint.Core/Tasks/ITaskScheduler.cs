using System.Collections.Generic;
using Blueprint.Core.ThirdParty;

namespace Blueprint.Core.Tasks
{
    [UsedImplicitly]
    public interface ITaskScheduler
    {
        IEnumerable<TaskSchedule> GetTaskSchedules();
    }
}
