using System.Threading.Tasks;

namespace Blueprint.Core.Tasks
{
    /// <summary>
    /// The post-processor that will apply to task execution, handling things such as saving data
    /// modified (transactional) and pushing new tasks that have been enqueued.
    /// </summary>
    public interface IBackgroundTaskExecutionPostProcessor
    {
        /// <summary>
        /// Performs any necessary work after the processing of the specified task has successfully
        /// completed.
        /// </summary>
        /// <param name="backgroundTask">The task that has been successfully executed.</param>
        /// <returns>A task representing the execution.</returns>
        Task PostProcessAsync(BackgroundTask backgroundTask);
    }
}