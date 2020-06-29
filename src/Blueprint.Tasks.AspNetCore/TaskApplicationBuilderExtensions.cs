using Blueprint;
using Blueprint.Tasks;
using Blueprint.Tasks.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extensions to <see cref="IApplicationBuilder" /> to register middleware that can
    /// execute background tasks.
    /// </summary>
    public static class TaskApplicationBuilderExtensions
    {
        /// <summary>
        /// Registers a middleware that will call <see cref="IBackgroundTaskScheduler.RunNowAsync" /> for the
        /// current instance of <see cref="IBackgroundTaskScheduler" /> after subsequent middleware has been
        /// executed.
        /// </summary>
        /// <param name="app">The builder to register with.</param>
        /// <seealso cref="TasksServiceCollectionExtensions.AddTasksClient" />
        public static void UseBlueprintTaskRunner(this IApplicationBuilder app)
        {
            Guard.NotNull(nameof(app), app);

            app.UseMiddleware<TaskRunnerMiddleware>();
        }
    }
}
