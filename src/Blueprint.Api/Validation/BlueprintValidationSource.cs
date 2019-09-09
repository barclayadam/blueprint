using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;
using Blueprint.Core.Utilities;
using Blueprint.Core.Validation;

namespace Blueprint.Api.Validation
{
    public class BlueprintValidationSource : IAttributeValidationSource
    {
        public async Task AddAttributeValidationResultsAsync(PropertyInfo propertyInfo, object value, ApiOperationContext apiOperationContext, ValidationFailures results)
        {
            var attributes = propertyInfo.GetAttributes<BlueprintValidationAttribute>(true);

            foreach (var attribute in attributes)
            {
                var result = await attribute.GetValidationResultAsync(propertyInfo.GetValue(value, null), propertyInfo.Name, apiOperationContext);

                if (result != ValidationResult.Success)
                {
                    results.AddFailure(result);
                }
            }
        }
    }
}
