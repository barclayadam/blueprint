using System;
using System.Threading.Tasks;

namespace Blueprint.Core.Apm
{
    /// <summary>
    /// An abstraction over an APM tool.
    /// </summary>
    public interface IApmTool
    {
        /// <summary>
        /// Executes the given operation, tracking it in the APM tool using the specified operation name.
        /// </summary>
        /// <param name="operationName">The name of the operation being performed.</param>
        /// <param name="executor">The method to execute.</param>
        /// <returns>The task from the child Func.</returns>
        Task InvokeAsync(string operationName, Func<Task> executor);

        /// <summary>
        /// Starts tracking a dependency, using the provided data to identify the dependency, returning a
        /// <see cref="IDisposable" /> that will, when disposed, actually store the tracking information.
        /// </summary>
        /// <param name="operationName">The name of the dependency being tracked.</param>
        /// <param name="target">The target of the dependency, for example a host name of a server.</param>
        /// <param name="type">The type of dependency, for example SQL or HTTP.</param>
        /// <param name="extraData">Any extra data that should be stored, for example a SQL statement or POST body.</param>
        /// <param name="executor">A method that represents the dependency.</param>
        /// <returns>The task from the child Func.</returns>
        Task TrackDependencyAsync(string operationName, string target, string type, string extraData, Func<IApmDependencyOperation, Task> executor);
    }
}