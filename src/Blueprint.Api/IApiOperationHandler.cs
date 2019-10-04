using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Blueprint.Api
{
    /// <summary>
    /// A handler for a specific <see cref="IApiOperation"/>.
    /// </summary>
    /// <typeparam name="T">The type of API operation this component handles.</typeparam>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public interface IApiOperationHandler<in T> where T : IApiOperation
    {
        Task<object> Invoke(T operation, ApiOperationContext apiOperationContext);
    }
}
