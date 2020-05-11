using System;
using System.Threading.Tasks;

namespace Blueprint.Tasks
{
    /// <summary>
    /// A background task 'scheduler', something that can take an instance of <see cref="IBackgroundTask" />
    /// and push it's execution to a background process.
    /// </summary>
    /// <remarks>
    /// A task represents an action that should be performed out-of-process, and has
    /// no need to be executed immediately (e.g. it may be stored and executed at a later time
    /// with no negative impact on the system and its operation).
    /// </remarks>
    /// <remarks>
    /// A task scheduler will queue up the tasks that are  passed and only attempt to execute / schedule them when
    /// <see cref="RunNowAsync" /> is called, allowing for tasks to be scheduled at any time during a request but
    /// only be "released" when the rest of the request / transaction is successful.
    /// </remarks>
    public interface IBackgroundTaskScheduler
    {
        /// <summary>
        /// Enqueues the given task, but <b>does not</b> immediately execute it as that is done through a call
        /// to <see cref="RunNowAsync" />.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <typeparam name="T">The exact type of the task being executed, usually can be inferred by compiler.</typeparam>
        /// <returns>A scheduled task that can be used to perform further operations, such as adding
        /// a continuation task.</returns>
        IScheduledBackgroundTask Enqueue<T>(T task) where T : IBackgroundTask;

        /// <summary>
        /// Scheduled the given task to be executed after a specified delay, but <b>does not</b> immediately
        /// do so, only being scheduled through a call to <see cref="RunNowAsync" />.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="delay">The amount of time to wait before executing this task.</param>
        /// <typeparam name="T">The exact type of the task being executed, usually can be inferred by compiler.</typeparam>
        /// <returns>A scheduled task that can be used to perform further operations, such as adding
        /// a continuation task.</returns>
        IScheduledBackgroundTask Schedule<T>(T task, TimeSpan delay) where T : IBackgroundTask;

        /// <summary>
        /// Runs all enqueued and scheduled tasks that have been queued up with this task scheduler.
        /// </summary>
        /// <returns>A task representing this operation.</returns>
        Task RunNowAsync();
    }
}
