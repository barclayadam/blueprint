using System.Threading.Tasks;
using Blueprint.Authorisation;

namespace Blueprint
{
    public interface IApiAuthoriserAggregator
    {
        ValueTask<ExecutionAllowed> CanShowLinkAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object resource);
    }
}
