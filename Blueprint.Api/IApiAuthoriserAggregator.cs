using System.Threading.Tasks;
using Blueprint.Api.Authorisation;

namespace Blueprint.Api
{
    public interface IApiAuthoriserAggregator
    {
        /// <see cref="IApiAuthoriser.CanShowLinkAsync"/>
        Task<ExecutionAllowed> CanShowLinkAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object resource);
    }
}
