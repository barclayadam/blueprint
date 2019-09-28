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
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using NLog;

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

        private readonly IServiceProvider serviceProvider;
        private readonly IErrorLogger errorLogger;
        private readonly IApmTool apmTool;

        /// <summary>
        /// Instantiates a new instance of the class TaskExecutor.
        /// </summary>
        /// <param name="serviceProvider">The parent container.</param>
        /// <param name="errorLogger">Error logger to track thrown exceptions.</param>
        /// <param name="apmTool">APM operation tracker to track individual task executions.</param>
        public TaskExecutor(
            IServiceProvider serviceProvider,
            IErrorLogger errorLogger,
            IApmTool apmTool)
        {
            Guard.NotNull(nameof(serviceProvider), serviceProvider);
            Guard.NotNull(nameof(errorLogger), errorLogger);
            Guard.NotNull(nameof(apmTool), apmTool);

            this.serviceProvider = serviceProvider;
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
                using (var nestedContainer = serviceProvider.CreateScope())
                {
                    await apmTool.InvokeAsync(GetOperationName(backgroundTask), async () =>
                    {
                        var handler = nestedContainer.ServiceProvider.GetService<IBackgroundTaskHandler<TTask>>();

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

                        var contextProvider = nestedContainer.ServiceProvider.GetRequiredService<IBackgroundTaskContextProvider>();
                        var contextKey = typeName;
                        var backgroundContext = new BackgroundTaskContext(contextKey, contextProvider);

                        await handler.HandleAsync(backgroundTask, backgroundContext);

                        await backgroundContext.SaveAsync();

                        var postProcessor = nestedContainer.ServiceProvider.GetRequiredService<IBackgroundTaskExecutionPostProcessor>();
                        await postProcessor.PostProcessAsync(backgroundTask);
                    });
                }
            }
            catch (Exception e)
            {
                if (errorLogger.ShouldIgnore(e))
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

                errorLogger.Log(e, errorData);

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

        /// <summary>
        /// Gets the maximum number of attempts allowed, which is the minimum <see cref="AutomaticRetryAttribute.Attempts" />
        /// of all registered filters of type <see cref="AutomaticRetryAttribute"/>
        /// </summary>
        /// <returns>Maximum number of retry attempts allowed.</returns>
        private static int GetMaxAttempts()
        {
            int? attempts = null;

            foreach (var att in GlobalJobFilters.Filters.OfType<AutomaticRetryAttribute>())
            {
                if (att.Attempts < (attempts ?? int.MaxValue))
                {
                    attempts = att.Attempts;
                }
            }

            return attempts ?? 0;
        }
    }
}
