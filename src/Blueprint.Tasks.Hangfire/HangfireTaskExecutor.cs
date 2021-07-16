using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Blueprint.Errors;
using Hangfire.Server;

namespace Blueprint.Tasks.Hangfire
{
    /// <summary>
    /// Resolves an appropriate task handler and allows it to perform the required action for the task.
    /// </summary>
    public class HangfireTaskExecutor
    {
        private readonly TaskExecutor _taskExecutor;
        private readonly IErrorLogger _errorLogger;

        /// <summary>
        /// Instantiates a new instance of the TaskExecutor class.
        /// </summary>
        /// <param name="taskExecutor">The task executor.</param>
        /// <param name="errorLogger">Error logger to track thrown exceptions.</param>
        public HangfireTaskExecutor(
            TaskExecutor taskExecutor,
            IErrorLogger errorLogger)
        {
            Guard.NotNull(nameof(taskExecutor), taskExecutor);
            Guard.NotNull(nameof(errorLogger), errorLogger);

            this._taskExecutor = taskExecutor;
            this._errorLogger = errorLogger;
        }

        /// <summary>
        /// Resolves a task handler for the given command context and, if found, hands off
        /// execution to the command handler.
        /// </summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="context">The Hangfire <see cref="PerformContext" />.</param>
        /// <param name="token">Token to indicate this task should be terminated.</param>
        /// <returns>A <see cref="Task" /> representing the execution of the given task.</returns>
        [DisplayName("{0}")]
        public async Task Execute(HangfireBackgroundTaskWrapper task, PerformContext context, CancellationToken token)
        {
            Guard.NotNull(nameof(task), task);

            var attempt = context.GetJobParameter<int?>("RetryCount");

            try
            {
                await this._taskExecutor.Execute(
                    task.Envelope,
                    s =>
                    {
                        s.SetTag("messaging.system", "hangfire");
                        s.SetTag("messaging.destination_kind", "queue");
                        s.SetTag("messaging.message_id", context.BackgroundJob.Id);

                        s.SetTag("hangfire.retryAttempt", (attempt ?? 1).ToString());
                    },
                    token);
            }
            catch (Exception e)
            {
                if (this._errorLogger.ShouldIgnore(e))
                {
                    return;
                }

                throw;
            }
        }
    }
}
