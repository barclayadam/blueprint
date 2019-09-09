using System.Threading.Tasks;

using Blueprint.Core.Api.Authorisation;

namespace Blueprint.Core.Api
{
    public interface IApiAuthoriserAggregator
    {
        /// <see cref="IApiAuthoriser.CanShowLinkAsync"/>
        Task<ExecutionAllowed> CanShowLinkAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object resource);
    }
}
