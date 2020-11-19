using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blueprint.Errors;
using Hangfire;
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
                        s.SetTag("hangfire.jobId", context.BackgroundJob.Id);
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

                // If this was not the last attempt then we will _not_ attempt to record this exception
                // but will instead just throw to retry. This is designed to reduce intermittent noise
                // of transient errors.
                if (attempt != null && attempt < GetMaxAttempts())
                {
                    throw;
                }

                e.Data["RetryCount"] = attempt?.ToString();
                e.Data["HangfireJobId"] = context.BackgroundJob.Id;

                await this._errorLogger.LogAsync(e);

                throw;
            }
        }

        /// <summary>
        /// Gets the maximum number of attempts allowed, which is the minimum <see cref="AutomaticRetryAttribute.Attempts" />
        /// of all registered filters of type <see cref="AutomaticRetryAttribute"/>.
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
