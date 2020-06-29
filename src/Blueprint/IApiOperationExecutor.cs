using System.Threading;
using System.Threading.Tasks;

namespace Blueprint
{
    public interface IApiOperationExecutor
    {
        /// <summary>
        /// Gets the <see cref="ApiDataModel" /> that this operation executor has been configured for. Any operations
        /// that are not registered with this model <strong>cannot</strong> be executed by this <see cref="IApiOperationExecutor"/>.
        /// </summary>
        ApiDataModel DataModel { get; }

        Task<OperationResult> ExecuteAsync(ApiOperationContext context);

        Task<OperationResult> ExecuteWithNewScopeAsync<T>(T operation, CancellationToken token = default) where T : IApiOperation;
    }
}
