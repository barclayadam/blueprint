using Blueprint.Core;
using Blueprint.Core.Tasks;
using Blueprint.Hangfire;

// This is the recommendation from MS for extensions to IApplicationBuilder to aid discoverability
// ReSharper disable once CheckNamespace
namespace Blueprint.Api.Configuration
{
    public static class BlueprintBackgroundTasksConfigurerExtensions
    {
        public static BlueprintBackgroundTasksConfigurer UseHangfire(this BlueprintBackgroundTasksConfigurer configurer)
        {
            Guard.NotNull(nameof(configurer), configurer);

            configurer.UseBackgroundTaskScheduleProvider<HangfireBackgroundTaskScheduleProvider>();
            configurer.UseBackgroundTaskScheduleProvider<ActivityTrackingBackgroundTaskScheduleProvider>();

            return configurer;
        }
    }
}
