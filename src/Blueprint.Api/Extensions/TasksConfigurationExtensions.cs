using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Api.Middleware;
using Blueprint.Core;
using Blueprint.Core.Auditing;
using Blueprint.Core.Tasks;

// This is the recommendation from MS for extensions to IServiceCollection to aid discoverability
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class TasksConfigurationExtensions
    {
        public static BlueprintApiBuilder AddBackgroundTasks(
            this BlueprintApiBuilder builder,
            Action<BlueprintTasksConfigurer> configureTasks)
        {
            EnsureNotAlreadySetup(builder.Services, typeof(IBackgroundTaskScheduler));

            configureTasks(new BlueprintTasksConfigurer(builder.Services));

            return builder;
        }

        /// <summary>
        /// Adds a middleware component to the <see cref="MiddlewareStage.PostExecution" /> stage that will push any
        /// background tasks from the operation to the registered provider.
        /// </summary>
        /// <seealso cref="AddBackgroundTasks" />
        /// <param name="pipelineBuilder">The configurer to push middleware to.</param>
        /// <returns><paramref name="pipelineBuilder"/>.</returns>
        public static BlueprintPipelineBuilder AddTaskRunner(this BlueprintPipelineBuilder pipelineBuilder)
        {
            pipelineBuilder.AddMiddleware<BackgroundTaskRunnerMiddleware>(MiddlewareStage.PostExecution);

            return pipelineBuilder;
        }

        private static void EnsureNotAlreadySetup(IServiceCollection services, Type type)
        {
            if (services.FirstOrDefault(d => d.ServiceType == type) != null)
            {
                throw new InvalidOperationException("Blueprint has already been configured.");
            }
        }
    }
}
