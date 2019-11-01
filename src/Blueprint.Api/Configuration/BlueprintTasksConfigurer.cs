using Blueprint.Core.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blueprint.Api.Configuration
{
    public class BlueprintTasksConfigurer
    {
        public BlueprintTasksConfigurer(IServiceCollection services)
        {
            Services = services;

            Services.TryAddScoped<IBackgroundTaskScheduler, BackgroundTaskScheduler>();
            Services.TryAddSingleton<IBackgroundTaskContextProvider, InMemoryBackgroundTaskContextProvider>();
        }

        public IServiceCollection Services { get; }

        public void UseProvider<T>() where T : class, IBackgroundTaskScheduleProvider
        {
            // Register the concrete type, and then register the "real" provider that is `ActivityTrackingBackgroundTaskScheduleProvider`, with
            // the provided T as it's inner implementation
            Services.AddScoped<T, T>();
            Services.AddScoped<IBackgroundTaskScheduleProvider>(c => new ActivityTrackingBackgroundTaskScheduleProvider(c.GetService<T>()));
        }
    }
}
