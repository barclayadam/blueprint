using System.Threading.Tasks;

namespace Blueprint.Api.Validation
{
    public interface IClassValidationSource : IValidationSource
    {
        Task AddClassValidationResultsAsync(object value, ApiOperationContext apiOperationContext, ValidationFailures results);
    }
}