using System.Threading.Tasks;

namespace Blueprint.Core.Api
{
    /// <summary>
    /// Provides an implementation of the async Invoke method of <see cref="IApiOperationHandler{T}"/> that
    /// will call an <see cref="InvokeSync" /> method which returns an object in a synchronous fashion.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SyncApiOperationHandler<T> : IApiOperationHandler<T> where T : IApiOperation
    {
        public Task<object> Invoke(T operation, ApiOperationContext apiOperationContext)
        {
            return Task.FromResult(InvokeSync(operation, apiOperationContext));
        }

        public abstract object InvokeSync(T operation, ApiOperationContext apiOperationContext);
    }
}