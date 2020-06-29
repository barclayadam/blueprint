using System.Threading.Tasks;

namespace Blueprint
{
    /// <summary>
    /// Defines an interface for a service which can execute a particular kind of <see cref="OperationResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of <see cref="OperationResult"/>.</typeparam>
    /// <remarks>
    /// Implementations of <see cref="IOperationResultExecutor{TResult}"/> are typically called by the
    /// <see cref="OperationResult.ExecuteAsync"/> method of the corresponding action result type.
    /// Implementations should be registered as singleton services.
    /// </remarks>
    public interface IOperationResultExecutor<in TResult> where TResult : OperationResult
    {
        /// <summary>
        /// Asynchronously executes the action result.
        /// </summary>
        /// <param name="context">The <see cref="ApiOperationContext"/> associated with the current request.</param>
        /// <param name="result">The action result to execute.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ExecuteAsync(ApiOperationContext context, TResult result);
    }
}
