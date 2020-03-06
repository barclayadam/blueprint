using System;
using System.Threading.Tasks;

namespace Blueprint.Api.Middleware
{
    public interface IContextMetadataProvider
    {
        Task PopulateMetadataAsync(ApiOperationContext context, Action<string, object> add);
    }
}
