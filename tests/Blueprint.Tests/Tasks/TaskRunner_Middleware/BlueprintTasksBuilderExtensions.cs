using Blueprint.Tasks;
using Blueprint.Tasks.Provider;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Tests.Tasks.TaskRunner_Middleware
{
    public static class BlueprintTasksBuilderExtensions
    {
        public static BlueprintTasksBuilder UseInMemory(
            this BlueprintTasksBuilder builder,
            InMemoryBackgroundTaskScheduleProvider provider)
        {
            builder.Services.AddScoped<IBackgroundTaskScheduleProvider>(serviceProvider => provider);

            return builder;
        }
    }
}
