using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blueprint.Core;

namespace Blueprint.Api.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class BlueprintValidationAttribute : Attribute
    {
        private const string DefaultErrorMessage = "{0} for {1} failed.";

        public virtual string ErrorMessage { get; set; } = DefaultErrorMessage;

        public virtual async Task<ValidationResult> GetValidationResultAsync(object value, string propertyName, ApiOperationContext apiOperationContext)
        {
            Guard.NotNull(nameof(apiOperationContext), apiOperationContext);

            var validationResult = await IsValidAsync(value, propertyName, apiOperationContext);

            if (validationResult == true)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(string.Format(ErrorMessage, GetType().Name, propertyName));
        }

        protected virtual Task<bool> IsValidAsync(object value, string propertyName, ApiOperationContext apiOperationContext)
        {
            return Task.FromResult(true);
        }
    }
}
