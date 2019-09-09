using System.Threading.Tasks;

using Blueprint.Core.ThirdParty;

namespace Blueprint.Core.Api
{
    /// <summary>
    /// A handler for a specific <see cref="IApiOperation"/>.
    /// </summary>
    /// <typeparam name="T">The type of API operation this component handles.</typeparam>
    [UsedImplicitly]
    public interface IApiOperationHandler<in T> where T : IApiOperation
    {
        Task<object> Invoke(T operation, ApiOperationContext apiOperationContext);
    }
}