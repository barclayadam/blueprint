using System.Threading.Tasks;

namespace Blueprint.Api
{
    public interface IApiOperationExecutor
    {
        /// <summary>
        /// Gets the <see cref="ApiDataModel" /> that this operation executor has been configured for. Any operations
        /// that are not registered with this model <strong>cannot</strong> be executed by this <see cref="IApiOperationExecutor"/>.
        /// </summary>
        ApiDataModel DataModel { get; }

        Task<OperationResult> ExecuteAsync(ApiOperationContext context);

        Task<OperationResult> ExecuteWithNewScopeAsync<T>(T operation) where T : IApiOperation;
    }
}
