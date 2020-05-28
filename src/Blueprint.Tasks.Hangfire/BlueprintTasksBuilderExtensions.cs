using System;
using System.Linq;
using Blueprint.Tasks.Hangfire;
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
    public static class BlueprintTasksBuilderExtensions
    {
        /// <summary>
        /// Configures the client to use Hangfire. Hangfire itself needs to be configured external to
        /// this (i.e. <c>services.AddHangfire(c => ...)</c>).
        /// </summary>
        /// <param name="builder">The builder to configure.</param>
        /// <returns>This <see cref="BlueprintTasksClientBuilder" /> for further configuration.</returns>
        public static BlueprintTasksClientBuilder UseHangfire(this BlueprintTasksClientBuilder builder)
        {
            builder.Services.AddScoped<IBackgroundTaskScheduleProvider, HangfireBackgroundTaskScheduleProvider>();

            RemoveInstance(j => j.Instance is AutomaticRetryAttribute);
            GlobalJobFilters.Filters.Add(new TaskAutomaticRetryJobFilter(new AutomaticRetryAttribute { Attempts = 5 }), -20);

            return builder;
        }

        /// <summary>
        /// Configures the server to use Hangfire. Hangfire itself needs to be configured external to
        /// this (i.e. <c>services.AddHangfire(c => ...)</c> and <c>services.AddHangfireServer()</c>).
        /// </summary>
        /// <param name="builder">The builder to configure.</param>
        /// <returns>This <see cref="BlueprintTasksServerBuilder" /> for further configuration.</returns>
        public static BlueprintTasksServerBuilder UseHangfire(this BlueprintTasksServerBuilder builder)
        {
            builder.Services.AddScoped<IBackgroundTaskScheduleProvider, HangfireBackgroundTaskScheduleProvider>();
            builder.Services.AddScoped<IRecurringTaskProvider, HangfireRecurringTaskProvider>();

            builder.Services.AddScoped<HangfireTaskExecutor>();

            RemoveInstance(j => j.Instance is AutomaticRetryAttribute);
            GlobalJobFilters.Filters.Add(new TaskAutomaticRetryJobFilter(new AutomaticRetryAttribute { Attempts = 5 }), -20);

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
