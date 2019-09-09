using System.Reflection;
using System.Threading.Tasks;
using Blueprint.Core.Validation;

namespace Blueprint.Api.Validation
{
    public interface IAttributeValidationSource : IValidationSource
    {
        Task AddAttributeValidationResultsAsync(PropertyInfo propertyInfo, object value, ApiOperationContext apiOperationContext, ValidationFailures results);
    }
}