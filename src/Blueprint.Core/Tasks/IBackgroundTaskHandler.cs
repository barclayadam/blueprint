using System.Threading.Tasks;

using Blueprint.Core.ThirdParty;

namespace Blueprint.Core.Tasks
{
    /// <summary>
    /// Represents a task handler, something that knows how to execute a given type
    /// of <see cref="BackgroundTask"/>.
    /// </summary>
    /// <typeparam name="T">The type of the task this handler can execute.</typeparam>
    [UsedImplicitly]
    public interface IBackgroundTaskHandler<in T> where T : BackgroundTask
    {
        /// <summary>
        /// Handles the given task, performing any necessary actions required to action
        /// the task.
        /// </summary>
        /// <remarks>
        /// A task will be deemed successful if no exception is raised from this method, meaning
        /// a failure <b>must</b> be indicated by an exception being thrown and that a task should
        /// typically no perform much exceptional error handling itself, instead allowing the
        /// client to perform those checks.
        /// </remarks>
        /// <param name="task">The task to be executed.</param>
        /// <param name="context">A context for the task _type_, allowing storing task-type specific data for subsequent runs.</param>
        Task HandleAsync(T task, BackgroundTaskContext context);
    }
}
