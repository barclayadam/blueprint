using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

using Blueprint.Core.Apm;
using Blueprint.Core.Errors;
using Blueprint.Core.Utilities;

using NLog;

using Hangfire;
using Hangfire.Server;

using IContainer = StructureMap.IContainer;

namespace Blueprint.Core.Tasks
{
    /// <summary>
    /// Resolves an appropriate task handler and allows it to perform the required action for the task.
    /// </summary>
    public class TaskExecutor
    {
        private static readonly Logger Log = LogManager.GetLogger("Blueprint.Tasks");

        private static readonly MethodInfo InvokeTaskHandlerMethod = typeof(TaskExecutor)
            .GetMethod(nameof(InvokeTaskHandlerAsync), BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly IContainer container;
        private readonly IErrorLogger errorLogger;
        private readonly IApmTool apmTool;

        /// <summary>
        /// Instantiates a new instance of the class TaskExecutor.
        /// </summary>
        /// <param name="container">The parent container.</param>
        /// <param name="errorLogger">Error logger to track thrown exceptions.</param>
        /// <param name="apmTool">APM operation tracker to track individual task executions.</param>
        public TaskExecutor(
            IContainer container,
            IErrorLogger errorLogger,
            IApmTool apmTool)
        {
            Guard.NotNull(nameof(container), container);
            Guard.NotNull(nameof(errorLogger), errorLogger);
            Guard.NotNull(nameof(apmTool), apmTool);

            this.container = container;
            this.errorLogger = errorLogger;
            this.apmTool = apmTool;
        }

        /// <summary>
        /// Resolves a task handler for the given command context and, if found, hands off
        /// execution to the command handler.
        /// </summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="context">The Hangfire context.</param>
        [DisplayName("{0}")]
        public async Task Execute(BackgroundTask task, PerformContext context)
        {
            Guard.NotNull(nameof(task), task);

            await (Task) InvokeTaskHandlerMethod
                .MakeGenericMethod(task.GetType())
                .Invoke(this, new object[] { task, context });
        }

        private async Task InvokeTaskHandlerAsync<TTask>(TTask backgroundTask, PerformContext context) where TTask : BackgroundTask
        {
            Guard.NotNull(nameof(backgroundTask), backgroundTask);
            Guard.NotNull(nameof(context), context);

            var typeName = backgroundTask.GetType().Name;

            var activity = new Activity("Task_In")
                .SetParentId(backgroundTask.Metadata.RequestId)
                .AddTag("JobId", context.BackgroundJob.Id)
                .AddTag("TaskType", typeName);

            if (backgroundTask.Metadata.RequestBaggage != null)
            {
                foreach (var pair in backgroundTask.Metadata.RequestBaggage)
                {
                    activity.AddBaggage(pair.Key, pair.Value);
                }
            }

            try
            {
                activity.Start();

                using (MappedDiagnosticsLogicalContext.SetScoped("Hangfire_JobId", context.BackgroundJob.Id))
                using (var nestedContainer = container.GetNestedContainer())
                {
                    await apmTool.InvokeAsync(GetOperationName(backgroundTask), async () =>
                    {
                        var handler = nestedContainer.GetInstance<IBackgroundTaskHandler<TTask>>();

                        if (handler == null)
                        {
                            throw new NoTaskHandlerFoundException(
                                $"No task handler found for type '{typeName}'.");
                        }

                        var enableConfigKey = $"Task.{typeName}.Enabled";

                        if (enableConfigKey.TryGetAppSetting(out bool isEnabled) && !isEnabled)
                        {
                            Log.Warn($"Task disabled in configuration. task_type={typeName} handler_type={handler.GetType().Name}");

                            return;
                        }

                        if (Log.IsTraceEnabled)
                        {
                            Log.Trace(
                                "Executing task in new nested container context. task_type={0} handler={1}",
                                backgroundTask.GetType().Name,
                                handler.GetType().Name);
                        }

                        var contextProvider = nestedContainer.GetInstance<IBackgroundTaskContextProvider>();
                        var contextKey = typeName;
                        var backgroundContext = new BackgroundTaskContext(contextKey, contextProvider);

                        await handler.HandleAsync(backgroundTask, backgroundContext);

                        await backgroundContext.SaveAsync();

                        var postProcessor = nestedContainer.GetInstance<IBackgroundTaskExecutionPostProcessor>();
                        await postProcessor.PostProcessAsync(backgroundTask);
                    });
                }
            }
            catch (Exception e)
            {
                if (this.errorLogger.ShouldIgnore(e))
                {
                    return;
                }

                // If this was not the last attempt then we will _not_ attempt to record this exception
                // but will instead just throw to retry. This is designed to reduce intermittent noise
                // of transient errors.
                var attempt = context.GetJobParameter<int?>("RetryCount");

                if (attempt != null && attempt < GetMaxAttempts())
                {
                    throw;
                }

                var errorData = new Dictionary<string, string>()
                {
                    ["RetryCount"] = attempt?.ToString(),
                    ["HangfireJobId"] = context.BackgroundJob.Id
                };

                this.errorLogger.Log(e, errorData);

                throw;
            }
            finally
            {
                activity.Stop();
            }
        }

        private static string GetOperationName<TTask>(TTask backgroundTask) where TTask : BackgroundTask
        {
            var categorisedTask = backgroundTask as IHaveTaskCategory;
            var taskType = backgroundTask.GetType();

            return categorisedTask != null ? taskType.Name + "-" + categorisedTask.Category : taskType.Name;
        }

        private static int GetMaxAttempts()
        {
            var autoRetryAttributes = GlobalJobFilters.Filters.OfType<AutomaticRetryAttribute>();

            if (!autoRetryAttributes.Any())
            {
                return 0;
            }

            return autoRetryAttributes.OrderBy(a => a.Attempts).First().Attempts;
        }
    }
}
