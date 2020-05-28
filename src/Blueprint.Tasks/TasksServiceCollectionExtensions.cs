using System;
using Blueprint.Tasks;
using Blueprint.Tasks.Provider;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Adds background task processing client to <see cref="IServiceCollection" />.
    /// </summary>
    public static class TasksServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a Tasks 'client', enabling injection of a <see cref="IBackgroundTaskScheduler" /> in to API
        /// endpoints that allows them to enqueue (or schedule for later execution) new <see cref="IBackgroundTask" />s.
        /// </summary>
        /// <param name="services">The services to configure.</param>
        /// <param name="configureTasks">A builder callback to configure the provider implementation for tasks.</param>
        public static void AddTasksClient(
            this IServiceCollection services,
            Action<BlueprintTasksClientBuilder> configureTasks)
        {
            RegisterClientServices(services);

            configureTasks(new BlueprintTasksClientBuilder(services));
        }

        internal static void RegisterClientServices(IServiceCollection services)
        {
            services.AddScoped<IBackgroundTaskScheduler, BackgroundTaskScheduler>();
            services.TryAddSingleton<IBackgroundTaskContextProvider, InMemoryBackgroundTaskContextProvider>();
        }
    }
}
