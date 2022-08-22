using System.Reflection;
using System.Threading.Tasks;

namespace Blueprint.Validation;

public interface IAttributeValidationSource : IValidationSource
{
    Task AddAttributeValidationResultsAsync(PropertyInfo propertyInfo, object value, ApiOperationContext apiOperationContext, ValidationFailures results);
}