using Blueprint.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Tasks
{
    /// <summary>
    /// An <see cref="IBlueprintApiHost" /> added when using HTTP (see <see cref="TasksConfigurationExtensions.AddTasksServer" />).
    /// </summary>
    public class TaskExecutorBlueprintApiHost : IBlueprintApiHost
    {
    }
}
