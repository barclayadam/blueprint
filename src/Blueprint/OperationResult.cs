using System.Threading.Tasks;

namespace Blueprint
{
    /// <summary>
    /// Represents the result of an operation that has passed through the API pipeline, providing a means
    /// of execution against an output destination (i.e. HTTP output stream) depending on what
    /// is to be achieved.
    /// </summary>
    public abstract class OperationResult
    {
        /// <summary>
        /// Executes this result.
        /// </summary>
        /// <param name="context">The context representing the request and response.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public abstract Task ExecuteAsync(ApiOperationContext context);
    }
}
