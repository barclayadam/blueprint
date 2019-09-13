using Blueprint.Core.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Configuration
{
    public class BlueprintBackgroundTasksConfigurer
    {
        private readonly BlueprintConfigurer blueprintConfigurer;

        public BlueprintBackgroundTasksConfigurer(BlueprintConfigurer blueprintConfigurer)
        {
            this.blueprintConfigurer = blueprintConfigurer;

            blueprintConfigurer.Services.AddScoped<IBackgroundTaskScheduler, BackgroundTaskScheduler>();
        }

        public IServiceCollection Services => blueprintConfigurer.Services;

        public void UseBackgroundTaskScheduleProvider<T>() where T : class, IBackgroundTaskScheduleProvider
        {
            Services.AddScoped<IBackgroundTaskScheduleProvider, T>();
        }
    }
}
