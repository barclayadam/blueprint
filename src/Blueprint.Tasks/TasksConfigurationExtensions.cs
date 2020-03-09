using System;
using System.Linq;
using Blueprint.Api.Configuration;
using Blueprint.Api.Middleware;
using Blueprint.Tasks;
using Blueprint.Tasks.Provider;
using Microsoft.Extensions.DependencyInjection.Extensions;

// This is the recommendation from MS for extensions to IServiceCollection to aid discoverability
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class TasksConfigurationExtensions
    {
        public static BlueprintApiBuilder AddTasksClient(
            this BlueprintApiBuilder builder,
            Action<BlueprintTasksClientBuilder> configureTasks)
        {
            RegisterClient(builder);

            configureTasks(new BlueprintTasksClientBuilder(builder));

            return builder;
        }

        public static BlueprintApiBuilder AddTasksServer(
            this BlueprintApiBuilder builder,
            Action<BlueprintTasksServerBuilder> configureTasks)
        {
            RegisterClient(builder);

            builder.UseHost(new TaskExecutorBlueprintApiHost());
            builder.Operations(o => o.AddConvention(new TasksOperationScannerConvention()));

            builder.Services.AddSingleton<TaskExecutor>();

            configureTasks(new BlueprintTasksServerBuilder(builder));

            return builder;
        }

        private static void RegisterClient(BlueprintApiBuilder builder)
        {
            // We will always add this to the pipeline. It is safe to do so as if no tasks are enqueued
            // this is in affect a no-op.
            builder.Pipeline(p => p.AddMiddleware<BackgroundTaskRunnerMiddleware>(MiddlewareStage.PostExecution));

            builder.Services.AddScoped<IBackgroundTaskScheduler, BackgroundTaskScheduler>();
            builder.Services.TryAddSingleton<IBackgroundTaskContextProvider, InMemoryBackgroundTaskContextProvider>();
        }
    }
}
