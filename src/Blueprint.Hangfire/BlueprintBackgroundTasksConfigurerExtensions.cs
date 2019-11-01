using Blueprint.Core;
using Blueprint.Core.Tasks;
using Blueprint.Hangfire;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

// This is the recommendation from MS for extensions to IApplicationBuilder to aid discoverability
// ReSharper disable once CheckNamespace
namespace Blueprint.Api.Configuration
{
    public static class BlueprintBackgroundTasksConfigurerExtensions
    {
        public static BlueprintTasksConfigurer UseHangfire(this BlueprintTasksConfigurer configurer)
        {
            Guard.NotNull(nameof(configurer), configurer);

            configurer.Services.AddSingleton<IBackgroundTaskScheduleProvider, HangfireBackgroundTaskScheduleProvider>();

            configurer.UseProvider<HangfireBackgroundTaskScheduleProvider>();

            return configurer;
        }
    }
}
