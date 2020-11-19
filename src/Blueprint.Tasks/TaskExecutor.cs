using System;
using System.Threading;
using System.Threading.Tasks;
using Blueprint.Apm;
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
        private readonly IApiOperationExecutor _apiOperationExecutor;
        private readonly IServiceProvider _rootServiceProvider;
        private readonly IApmTool _apmTool;

        /// <summary>
        /// Initialises a new instance of the <see cref="TaskExecutor" /> class.
        /// </summary>
        /// <param name="apiOperationExecutor">The API operation executor that can handle incoming background tasks.</param>
        /// <param name="rootServiceProvider">The root <see cref="IServiceProvider" /> of the application.</param>
        /// <param name="apmTool">The APM integration.</param>
        public TaskExecutor(
            IApiOperationExecutor apiOperationExecutor,
            IServiceProvider rootServiceProvider,
            IApmTool apmTool)
        {
            this._apiOperationExecutor = apiOperationExecutor;
            this._rootServiceProvider = rootServiceProvider;
            this._apmTool = apmTool;
        }

        /// <summary>
        /// Pushes the given <see cref="IBackgroundTask" /> that has been wrapped in an <see cref="BackgroundTaskEnvelope" />
        /// to a <see cref="IApiOperationExecutor" />.
        /// </summary>
        /// <param name="taskEnvelope">The task to be executed.</param>
        /// <param name="configureSpan">An action that will be called with an <see cref="IApmSpan" /> for the service-specific
        /// provider to add tags.</param>
        /// <param name="token">A cancellation token that indicates this method should be aborted.</param>
        /// <returns>A <see cref="Task" /> representing the execution of the given task.</returns>
        public async Task Execute(
            BackgroundTaskEnvelope taskEnvelope,
            Action<IApmSpan> configureSpan,
            CancellationToken token)
        {
            Guard.NotNull(nameof(taskEnvelope), taskEnvelope);

            using var nestedContainer = this._rootServiceProvider.CreateScope();

            var apiContext = new ApiOperationContext(
                nestedContainer.ServiceProvider,
                this._apiOperationExecutor.DataModel,
                taskEnvelope.Task,
                token);

            using var span = this._apmTool.StartOperation(
                apiContext.Descriptor,
                SpanKinds.Consumer,
                taskEnvelope.ApmContext);

            configureSpan(span);

            apiContext.ApmSpan = span;

            var result = await this._apiOperationExecutor.ExecuteAsync(apiContext);

            if (result is UnhandledExceptionOperationResult unhandledExceptionOperationResult)
            {
                span.RecordException(unhandledExceptionOperationResult.Exception);

                unhandledExceptionOperationResult.Rethrow();
            }
        }
    }
}
