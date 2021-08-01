using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blueprint.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Blueprint.Tasks
{
    /// <summary>
    /// The main entry point in to background task processing, taking a <see cref="BackgroundTaskEnvelope" /> that
    /// has been scheduled via <see cref="IBackgroundTaskScheduler" /> or a recurring one through <see cref="RecurringTaskManager" />
    /// and pushing it through an <see cref="IApiOperationExecutor" />.
    /// </summary>
    public class TaskExecutor
    {
        /// <summary>
        /// The <see cref="ActivitySource" /> used for task-related activities.
        /// </summary>
        public static readonly ActivitySource ActivitySource = BlueprintActivitySource.CreateChild(typeof(TaskExecutor), "Tasks");

        /// <summary>
        /// A propagator used to push Activity data through to consumers for correlation.
        /// </summary>
        public static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        private readonly IApiOperationExecutor _apiOperationExecutor;
        private readonly IServiceProvider _rootServiceProvider;

        /// <summary>
        /// Initialises a new instance of the <see cref="TaskExecutor" /> class.
        /// </summary>
        /// <param name="apiOperationExecutor">The API operation executor that can handle incoming background tasks.</param>
        /// <param name="rootServiceProvider">The root <see cref="IServiceProvider" /> of the application.</param>
        public TaskExecutor(
            IApiOperationExecutor apiOperationExecutor,
            IServiceProvider rootServiceProvider)
        {
            this._apiOperationExecutor = apiOperationExecutor;
            this._rootServiceProvider = rootServiceProvider;
        }

        /// <summary>
        /// Pushes the given <see cref="IBackgroundTask" /> that has been wrapped in an <see cref="BackgroundTaskEnvelope" />
        /// to a <see cref="IApiOperationExecutor" />.
        /// </summary>
        /// <param name="taskEnvelope">The task to be executed.</param>
        /// <param name="configureActivity">An action that will be called with an <see cref="Activity" /> for the service-specific
        /// provider to add tags.</param>
        /// <param name="token">A cancellation token that indicates this method should be aborted.</param>
        /// <returns>A <see cref="Task" /> representing the execution of the given task.</returns>
        public async Task Execute(
            BackgroundTaskEnvelope taskEnvelope,
            Action<Activity> configureActivity,
            CancellationToken token)
        {
            Guard.NotNull(nameof(taskEnvelope), taskEnvelope);

            var parentContext = Propagator.Extract(default, taskEnvelope.Headers, ExtractTraceContextFromBasicProperties);
            Baggage.Current = parentContext.Baggage;

            using var activity = ActivitySource.StartActivity(taskEnvelope.Task.GetType().Name, ActivityKind.Consumer, parentContext.ActivityContext);

            if (activity != null)
            {
                configureActivity(activity);
            }

            using var nestedContainer = this._rootServiceProvider.CreateScope();

            var apiContext = new ApiOperationContext(
                nestedContainer.ServiceProvider,
                this._apiOperationExecutor.DataModel,
                taskEnvelope.Task,
                token);

            var result = await this._apiOperationExecutor.ExecuteAsync(apiContext);

            if (result is UnhandledExceptionOperationResult unhandledExceptionOperationResult)
            {
                BlueprintActivitySource.RecordException(activity, unhandledExceptionOperationResult.Exception);

                unhandledExceptionOperationResult.Rethrow();
            }
        }

        private static IEnumerable<string> ExtractTraceContextFromBasicProperties(IDictionary<string, string> props, string key)
        {
            if (props.TryGetValue(key, out var value))
            {
                return new[] { value };
            }

            return Enumerable.Empty<string>();
        }
    }
}
