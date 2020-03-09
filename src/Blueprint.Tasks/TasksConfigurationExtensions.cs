using System;
using System.Threading;
using System.Threading.Tasks;
using Blueprint.Api.Configuration;
using Blueprint.Api.Middleware;
using Blueprint.Tasks;
using Blueprint.Tasks.Provider;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

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

            builder.Services.AddHostedService<RecurringJobManagerStartup>();

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

        /// <summary>
        /// An <see cref="IHostedService" /> that is responsible, on startup of the application, to install
        /// the recurring task manager service using <see cref="IRecurringTaskProvider.SetupRecurringManagerAsync" />.
        /// </summary>
        private class RecurringJobManagerStartup : IHostedService
        {
            private readonly IHostApplicationLifetime appLifetime;
            private readonly IServiceProvider serviceProvider;

            public RecurringJobManagerStartup(IHostApplicationLifetime appLifetime, IServiceProvider serviceProvider)
            {
                this.appLifetime = appLifetime;
                this.serviceProvider = serviceProvider;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                appLifetime.ApplicationStarted.Register(OnStarted);

                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            private async void OnStarted()
            {
                using var scope = serviceProvider.CreateScope();
                var provider = scope.ServiceProvider.GetRequiredService<IRecurringTaskProvider>();

                await provider.SetupRecurringManagerAsync();
            }
        }
    }
}
