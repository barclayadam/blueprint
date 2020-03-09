using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Core;
using Blueprint.Core.Errors;
using Blueprint.Tasks;
using Hangfire;
using Hangfire.Server;

namespace Blueprint.Hangfire
{
    /// <summary>
    /// Resolves an appropriate task handler and allows it to perform the required action for the task.
    /// </summary>
    public class HangfireTaskExecutor
    {
        private readonly TaskExecutor taskExecutor;
        private readonly IErrorLogger errorLogger;

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

            this.taskExecutor = taskExecutor;
            this.errorLogger = errorLogger;
        }

        /// <summary>
        /// Resolves a task handler for the given command context and, if found, hands off
        /// execution to the command handler.
        /// </summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="context">The Hangfire <see cref="PerformContext" />.</param>
        /// <returns>A <see cref="Task" /> representing the execution of the given task.</returns>
        [DisplayName("{0}")]
        public async Task Execute(HangfireBackgroundTaskWrapper task, PerformContext context)
        {
            Guard.NotNull(nameof(task), task);

            try
            {
                await taskExecutor.Execute(task.Envelope);
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

                e.Data["RetryCount"] = attempt?.ToString();
                e.Data["HangfireJobId"] = context.BackgroundJob.Id;

                errorLogger.Log(e);

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
