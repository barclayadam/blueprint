using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Tasks
{
    /// <summary>
    /// A builder that is used to configure the server-side of tasks feature.
    /// </summary>
    public class BlueprintTasksServerBuilder : BlueprintTasksBuilder
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="BlueprintTasksServerBuilder" /> class.
        /// </summary>
        /// <param name="serviceCollection">The service collection being configured.</param>
        public BlueprintTasksServerBuilder(IServiceCollection serviceCollection)
            : base(serviceCollection)
        {
        }

        /// <summary>
        /// Adds recurring tasks support, which will register the required hosted services and
        /// task managers needed.
        /// </summary>
        /// <remarks>
        /// Recurring tasks are defined by <see cref="IRecurringTaskScheduler" />s that can
        /// bre registered and provide CRON-based jobs that allow for the creation of
        /// <see cref="IBackgroundTask" />s that can be executed on a schedule.
        /// </remarks>
        /// <param name="configureOptions">An action to configure <see cref="RecurringTaskManagerOptions" />.</param>
        /// <returns>This builder for further configuration.</returns>
        public BlueprintTasksServerBuilder AddRecurringTasks([CanBeNull] Action<RecurringTaskManagerOptions> configureOptions = null)
        {
            this.Services.AddOptions<RecurringTaskManagerOptions>();
            this.Services.AddScoped<RecurringTaskManager>();
            this.Services.AddHostedService<RecurringJobManagerRegistrationHostedService>();

            if (configureOptions != null)
            {
                this.Services.Configure(configureOptions);
            }

            return this;
        }
    }
}
