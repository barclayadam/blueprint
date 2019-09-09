using System.Threading.Tasks;

using Blueprint.Core.Api;

namespace Blueprint.Core.Validation
{
    public interface IClassValidationSource : IValidationSource
    {
        Task AddClassValidationResultsAsync(object value, ApiOperationContext apiOperationContext, ValidationFailures results);
    }
}