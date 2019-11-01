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
        public static BlueprintApiConfigurer AddBackgroundTasks(
            this BlueprintApiConfigurer configurer,
            Action<BlueprintTasksConfigurer> configureTasks)
        {
            EnsureNotAlreadySetup(configurer.Services, typeof(IBackgroundTaskScheduler));

            configureTasks(new BlueprintTasksConfigurer(configurer.Services));

            return configurer;
        }

        /// <summary>
        /// Adds a middleware component to the <see cref="MiddlewareStage.PostExecution" /> stage that will push any
        /// background tasks from the operation to the registered provider.
        /// </summary>
        /// <seealso cref="AddBackgroundTasks" />
        /// <param name="middlewareConfigurer">The configurer to push middleware to.</param>
        /// <returns><paramref name="middlewareConfigurer"/>.</returns>
        public static BlueprintMiddlewareConfigurer AddTaskRunner(this BlueprintMiddlewareConfigurer middlewareConfigurer)
        {
            middlewareConfigurer.AddMiddleware<BackgroundTaskRunnerMiddleware>(MiddlewareStage.PostExecution);

            return middlewareConfigurer;
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
