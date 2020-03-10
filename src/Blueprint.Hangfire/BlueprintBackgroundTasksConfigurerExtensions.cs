using System;
using System.Linq;
using Blueprint.Hangfire;
using Blueprint.Tasks.Provider;
using Hangfire;
using Hangfire.Common;
using Microsoft.Extensions.DependencyInjection;

// Ensure this is discoverable so long as the project has been referenced
// ReSharper disable once CheckNamespace
namespace Blueprint.Tasks
{
    /// <summary>
    /// Extensions to <see cref="BlueprintTasksClientBuilder" /> and <see cref="BlueprintTasksServerBuilder" /> for
    /// installing Hangfire.
    /// </summary>
    public static class BlueprintBackgroundTasksConfigurerExtensions
    {
        /// <summary>
        /// Configures the client to use Hangfire, which should be configured using the given configuration
        /// builder (<paramref name="configuration"/>) for properties such as storage and queue mechanism.
        /// </summary>
        /// <param name="builder">The builder to configure.</param>
        /// <param name="configuration">The Hangfire-specific configuration.</param>
        /// <returns>This <see cref="BlueprintTasksClientBuilder" /> for further configuration.</returns>
        public static BlueprintTasksClientBuilder UseHangfire(
            this BlueprintTasksClientBuilder builder,
            Action<IGlobalConfiguration> configuration)
        {
            builder.Services.AddScoped<IBackgroundTaskScheduleProvider, HangfireBackgroundTaskScheduleProvider>();

            builder.Services.AddHangfire((s, c) =>
            {
                RemoveInstance(j => j.Instance is AutomaticRetryAttribute);
                GlobalJobFilters.Filters.Add(new TaskAutomaticRetryJobFilter(new AutomaticRetryAttribute { Attempts = 5 }), -20);

                c.UseServiceProviderActivator(s)
                    .UseRecommendedSerializerSettings();

                configuration(c);
            });

            return builder;
        }

        /// <summary>
        /// Configures the server to use Hangfire, which should be configured using the given configuration
        /// builder (<paramref name="configuration"/>) for properties such as storage and queue mechanism.
        /// </summary>
        /// <param name="builder">The builder to configure.</param>
        /// <param name="configuration">The Hangfire-specific configuration.</param>
        /// <returns>This <see cref="BlueprintTasksServerBuilder" /> for further configuration.</returns>
        public static BlueprintTasksServerBuilder UseHangfire(
            this BlueprintTasksServerBuilder builder,
            Action<IGlobalConfiguration> configuration)
        {
            builder.Services.AddScoped<IBackgroundTaskScheduleProvider, HangfireBackgroundTaskScheduleProvider>();
            builder.Services.AddSingleton<IRecurringTaskProvider, HangfireRecurringTaskProvider>();

            builder.Services.AddSingleton<HangfireTaskExecutor>();

            builder.Services.AddHangfire((s, c) =>
            {
                RemoveInstance(j => j.Instance is AutomaticRetryAttribute);
                GlobalJobFilters.Filters.Add(new TaskAutomaticRetryJobFilter(new AutomaticRetryAttribute { Attempts = 5 }), -20);

                c.UseServiceProviderActivator(s)
                 .UseRecommendedSerializerSettings();

                configuration(c);
            });

            builder.Services.AddHangfireServer();

            return builder;
        }

        private static void RemoveInstance(Func<JobFilter, bool> filterPredicate)
        {
            var filter = GlobalJobFilters.Filters.SingleOrDefault(filterPredicate);

            if (filter != null)
            {
                GlobalJobFilters.Filters.Remove(filter.Instance);
            }
        }
    }
}
