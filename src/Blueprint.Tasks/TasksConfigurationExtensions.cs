using System;
using Blueprint.Configuration;
using Blueprint.Tasks;

// Match the DI container namespace so that Blueprint is immediately discoverable
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Adds background task processing entry points to <see cref="BlueprintApiBuilder{THost}" />.
    /// </summary>
    public static class TasksConfigurationExtensions
    {
        /// <summary>
        /// Adds a Tasks 'client', enabling injection of a <see cref="IBackgroundTaskScheduler" /> in to API
        /// endpoints that allows them to enqueue (or schedule for later execution) new <see cref="IBackgroundTask" />s.
        /// </summary>
        /// <param name="builder">The builder to add the task client to.</param>
        /// <param name="configureTasks">A builder callback to configure the provider implementation for tasks.</param>
        /// <typeparam name="THost">The type of host.</typeparam>
        /// <returns>This builder for further configuration.</returns>
        /// <seealso cref="BackgroundTasks" />
        public static BlueprintApiBuilder<THost> AddTasksClient<THost>(
            this BlueprintApiBuilder<THost> builder,
            Action<BlueprintTasksClientBuilder> configureTasks)
        {
            RegisterClient(builder);

            configureTasks(new BlueprintTasksClientBuilder(builder.Services));

            return builder;
        }

        /// <summary>
        /// Adds a Tasks 'server', <see cref="TaskExecutor" /> implementations in
        /// addition to provider-specific functionality that enables the execution of <see cref="IBackgroundTask" />s in
        /// this application.
        /// </summary>
        /// <remarks>
        /// Note that by default we DO NOT register recurring task support.
        /// </remarks>
        /// <param name="builder">The builder to add the task server to.</param>
        /// <param name="configureTasks">A builder callback to configure the provider implementation for tasks.</param>
        /// <returns>A <see cref="BlueprintApiBuilder{BackgroundTasksHost}" /> for configuration.</returns>
        /// <seealso cref="AddTasksClient{THost}" />
        /// <seealso cref="BlueprintTasksServerBuilder.AddRecurringTasks" />
        public static BlueprintApiBuilder<BackgroundTasksHost> BackgroundTasks(
            this BlueprintApiHostBuilder builder,
            Action<BlueprintTasksServerBuilder> configureTasks)
        {
            var apiBuilder = builder.UseHost<BackgroundTasksHost>();

            RegisterClient(apiBuilder);

            apiBuilder.Operations(o => o.AddConvention(new TasksOperationScannerConvention()));

            apiBuilder.Services.AddScoped<TaskExecutor>();

            configureTasks(new BlueprintTasksServerBuilder(apiBuilder.Services));

            return apiBuilder;
        }

        private static void RegisterClient<THost>(BlueprintApiBuilder<THost> builder)
        {
            // We will always add this to the pipeline. It is safe to do so as if no tasks are enqueued
            // this is in affect a no-op.
            builder.Pipeline(p => p.AddMiddleware<BackgroundTaskRunnerMiddleware>(MiddlewareStage.PostExecution));

            TasksServiceCollectionExtensions.RegisterClientServices(builder.Services);
        }
    }
}
