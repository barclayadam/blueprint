using Blueprint.Core.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Configuration
{
    public class BlueprintTasksConfigurer
    {
        public BlueprintTasksConfigurer(IServiceCollection services)
        {
            Services = services;

            Services.AddScoped<IBackgroundTaskScheduler, BackgroundTaskScheduler>();
        }

        public IServiceCollection Services { get; }

        public void UseBackgroundTaskScheduleProvider<T>() where T : class, IBackgroundTaskScheduleProvider
        {
            Services.AddScoped<IBackgroundTaskScheduleProvider, T>();
        }
    }
}
