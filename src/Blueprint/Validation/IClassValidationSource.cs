using System.Threading.Tasks;

namespace Blueprint.Validation;

public interface IClassValidationSource : IValidationSource
{
    Task AddClassValidationResultsAsync(object value, ApiOperationContext apiOperationContext, ValidationFailures results);
}