using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blueprint.Core.ThirdParty;
using NJsonSchema;

namespace Blueprint.Api.Validation
{
    /// <summary>
    /// Ensures the property has a greater value than the 'dependant property'.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MustBeGreaterThanOrEqualAttribute : ValidationAttribute, IOpenApiValidationAttribute
    {
        private const string RequiredIfFieldMessage = "The {0} field must be greater than or equal to";

        public MustBeGreaterThanOrEqualAttribute(string dependentProperty)
            : base(RequiredIfFieldMessage + dependentProperty)
        {
            DependentProperty = dependentProperty;
        }

        /// <summary>
        /// Gets the property to check for one of the dependant values.
        /// </summary>
        public string DependentProperty { get; }

        public string ValidatorKeyword => "x-validator-greater-than-or-equal";

        public Task PopulateAsync(JsonSchema4 schema, ApiOperationContext apiOperationContext)
        {
            schema.ExtensionData[ValidatorKeyword] = new Dictionary<string, object>
            {
                ["$data"] = $"1/{DependentProperty.Camelize()}",
            };

            return Task.CompletedTask;
        }

        protected override ValidationResult IsValid(object maxValue, ValidationContext validationContext)
        {
            var property = validationContext.ObjectInstance.GetType().GetProperty(DependentProperty);
            if (property == null)
            {
                throw new InvalidOperationException($"{DependentProperty} was null!");
            }

            var item = property.GetValue(validationContext.ObjectInstance, null);

            if (item is IComparable minValue)
            {
                var result = minValue.CompareTo(maxValue);

                if (result <= 0)
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                throw new InvalidOperationException($"{DependentProperty} is not IComparable");
            }

            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), new[] { validationContext.DisplayName });
        }
    }
}
