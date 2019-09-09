using System;
using System.Threading.Tasks;

namespace Blueprint.Core.Apm
{
    /// <summary>
    /// A null <see cref="IApmTool" /> that performs no actual tracking.
    /// </summary>
    public class NullApmTool : IApmTool
    {
        /// <inheritdoc />
        public Task InvokeAsync(string operationName, Func<Task> executor)
        {
            return executor();
        }

        /// <inheritdoc />
        public Task TrackDependencyAsync(string operationName, string target, string type, string extraData, Func<IApmDependencyOperation, Task> executor)
        {
            return executor(NulloApmDependencyOperation.Instance);
        }
    }
}