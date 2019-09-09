using System.Threading.Tasks;
using Blueprint.Core.Validation;

namespace Blueprint.Api.Validation
{
    public interface IClassValidationSource : IValidationSource
    {
        Task AddClassValidationResultsAsync(object value, ApiOperationContext apiOperationContext, ValidationFailures results);
    }
}