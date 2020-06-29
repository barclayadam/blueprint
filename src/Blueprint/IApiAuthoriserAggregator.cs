using System.Threading.Tasks;
using Blueprint.Authorisation;

namespace Blueprint
{
    public interface IApiAuthoriserAggregator
    {
        Task<ExecutionAllowed> CanShowLinkAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object resource);
    }
}
