using System.Threading.Tasks;
using Blueprint.Core.Api;
using NJsonSchema;

namespace Blueprint.Core.OpenApi
{
    public interface IOpenApiValidationAttribute
    {
        string ValidatorKeyword { get; }

        Task PopulateAsync(JsonSchema4 schema, ApiOperationContext apiOperationContext);
    }
}
