using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;

using Blueprint.Core.Api;
using Blueprint.Core.Utilities;

namespace Blueprint.Core.Validation
{
    public class DataAnnotationsValidationSource : IAttributeValidationSource, IClassValidationSource
    {
        public Task AddAttributeValidationResultsAsync(PropertyInfo propertyInfo, object value, ApiOperationContext apiOperationContext, ValidationFailures results)
        {
            var validationContext = new ValidationContext(value, null, null);

            var validationAttributes = propertyInfo.GetAttributes<ValidationAttribute>(true);

            foreach (var validation in validationAttributes)
            {
                var result = validation.GetValidationResult(
                    propertyInfo.GetValue(value, null),
                    GetValidationContext(validationContext, propertyInfo));

                if (result != ValidationResult.Success)
                {
                    results.AddFailure(result);
                }
            }

            return Task.CompletedTask;
        }

        public Task AddClassValidationResultsAsync(object value, ApiOperationContext apiOperationContext, ValidationFailures results)
        {
            var validationContext = new ValidationContext(value, null, null);

            if (value is IValidatableObject validatableObject)
            {
                var validationResults = validatableObject.Validate(validationContext);

                foreach (var validationResult in validationResults)
                {
                    results.AddFailure(validationResult);
                }
            }

            return Task.CompletedTask;
        }

        private static ValidationContext GetValidationContext(ValidationContext validationContext, PropertyInfo propertyInfo)
        {
            validationContext.MemberName = propertyInfo.Name;
            validationContext.DisplayName = propertyInfo.Name;

            return validationContext;
        }
    }
}