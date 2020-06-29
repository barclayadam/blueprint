using System.Threading.Tasks;
using NJsonSchema;

namespace Blueprint.Validation
{
    public interface IOpenApiValidationAttribute
    {
        string ValidatorKeyword { get; }

        Task PopulateAsync(JsonSchema4 schema, ApiOperationContext apiOperationContext);
    }
}
