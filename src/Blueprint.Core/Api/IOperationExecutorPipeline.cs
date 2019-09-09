using System.Threading.Tasks;

namespace Blueprint.Core.Api
{
    /// <summary>
    /// The interface that will be implemented for each operation that exists within an <see cref="ApiDataModel" />
    /// when constructing an <see cref="IApiOperationExecutor" />, with the <see cref="Execute"/> method having been
    /// generated from registered <see cref="IMiddlewareBuilder"/>s.
    /// </summary>
    public interface IOperationExecutorPipeline
    {
        Task<OperationResult> Execute(ApiOperationContext context);
    }
}
