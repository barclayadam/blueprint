using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Tasks
{
    /// <summary>
    /// The main entry point in to background task processing, taking a <see cref="BackgroundTaskEnvelope" /> that
    /// has been scheduled via <see cref="IBackgroundTaskScheduler" /> or a recurring one through <see cref="RecurringTaskManager" />
    /// and pushing it through an <see cref="IApiOperationExecutor" />.
    /// </summary>
    public class TaskExecutor
    {
        private readonly IApiOperationExecutor apiOperationExecutor;
        private readonly IServiceProvider rootServiceProvider;

        /// <summary>
        /// Initialises a new instance of the <see cref="TaskExecutor" /> class.
        /// </summary>
        /// <param name="apiOperationExecutor">The API operation executor that can handle incoming background tasks.</param>
        /// <param name="rootServiceProvider">The root <see cref="IServiceProvider" /> of the application.</param>
        public TaskExecutor(
            IApiOperationExecutor apiOperationExecutor,
            IServiceProvider rootServiceProvider)
        {
            this.apiOperationExecutor = apiOperationExecutor;
            this.rootServiceProvider = rootServiceProvider;
        }

        /// <summary>
        /// Pushes the given <see cref="IBackgroundTask" /> that has been wrapped in an <see cref="BackgroundTaskEnvelope" />
        /// to a <see cref="IApiOperationExecutor" />.
        /// </summary>
        /// <param name="taskEnvelope">The task to be executed.</param>
        /// <param name="token">A cancellation token that indicates this method should be aborted.</param>
        /// <returns>A <see cref="Task" /> representing the execution of the given task.</returns>
        public async Task Execute(BackgroundTaskEnvelope taskEnvelope, CancellationToken token)
        {
            Guard.NotNull(nameof(taskEnvelope), taskEnvelope);

            var typeName = taskEnvelope.BackgroundTask.GetType().Name;

            var activity = new Activity("Task_In")
                .SetParentId(taskEnvelope.Metadata.RequestId)
                .AddTag("TaskType", typeName);

            if (taskEnvelope.Metadata.RequestBaggage != null)
            {
                foreach (var pair in taskEnvelope.Metadata.RequestBaggage)
                {
                    activity.AddBaggage(pair.Key, pair.Value);
                }
            }

            try
            {
                activity.Start();

                using var nestedContainer = rootServiceProvider.CreateScope();

                var apiContext = new ApiOperationContext(
                    nestedContainer.ServiceProvider,
                    apiOperationExecutor.DataModel,
                    taskEnvelope.BackgroundTask,
                    token);

                var result = await apiOperationExecutor.ExecuteAsync(apiContext);

                if (result is UnhandledExceptionOperationResult unhandledExceptionOperationResult)
                {
                    unhandledExceptionOperationResult.Rethrow();
                }
            }
            finally
            {
                activity.Stop();
            }
        }
    }
}
