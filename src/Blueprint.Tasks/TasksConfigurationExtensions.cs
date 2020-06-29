using System;
using Blueprint.Configuration;
using Blueprint.Tasks;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Blueprint.Api.Configuration
{
    /// <summary>
    /// Adds background task processing entry points to <see cref="BlueprintApiBuilder" />.
    /// </summary>
    public static class TasksConfigurationExtensions
    {
        /// <summary>
        /// Adds a Tasks 'client', enabling injection of a <see cref="IBackgroundTaskScheduler" /> in to API
        /// endpoints that allows them to enqueue (or schedule for later execution) new <see cref="IBackgroundTask" />s.
        /// </summary>
        /// <param name="builder">The builder to add the task client to.</param>
        /// <param name="configureTasks">A builder callback to configure the provider implementation for tasks.</param>
        /// <returns>This <see cref="BlueprintApiBuilder" /> for further configuration.</returns>
        /// <seealso cref="AddTasksServer" />
        public static BlueprintApiBuilder AddTasksClient(
            this BlueprintApiBuilder builder,
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
        /// <returns>This <see cref="BlueprintApiBuilder" /> for further configuration.</returns>
        /// <seealso cref="AddTasksClient" />
        /// <seealso cref="BlueprintTasksServerBuilder.AddRecurringTasks" />
        public static BlueprintApiBuilder AddTasksServer(
            this BlueprintApiBuilder builder,
            Action<BlueprintTasksServerBuilder> configureTasks)
        {
            RegisterClient(builder);

            builder.UseHost(new TaskExecutorBlueprintApiHost());
            builder.Operations(o => o.AddConvention(new TasksOperationScannerConvention()));

            builder.Services.AddScoped<TaskExecutor>();

            configureTasks(new BlueprintTasksServerBuilder(builder.Services));

            return builder;
        }

        private static void RegisterClient(BlueprintApiBuilder builder)
        {
            // We will always add this to the pipeline. It is safe to do so as if no tasks are enqueued
            // this is in affect a no-op.
            builder.Pipeline(p => p.AddMiddleware<BackgroundTaskRunnerMiddleware>(MiddlewareStage.PostExecution));

            TasksServiceCollectionExtensions.RegisterClientServices(builder.Services);
        }
    }
}
