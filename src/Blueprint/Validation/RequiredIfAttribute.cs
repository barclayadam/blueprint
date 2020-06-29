using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.ThirdParty;
using NJsonSchema;

namespace Blueprint.Validation
{
    /// <summary>
    /// Makes the property required if the 'dependant property' has a value that exists in the 'dependant value'.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequiredIfAttribute : ValidationAttribute, IOpenApiValidationAttribute
    {
        private const string RequiredIfFieldMessage = "The {0} field is required";

        private readonly IEnumerable<string> convertedValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="dependentProperty">The dependant property.</param>
        /// <param name="dependentValue">The dependant value(s).</param>
        public RequiredIfAttribute(string dependentProperty, object dependentValue)
            : base(RequiredIfFieldMessage + $@" if {dependentProperty} field is {dependentValue}.")
        {
            DependentProperty = dependentProperty;
            DependentValue = dependentValue;

            if (dependentValue != null && dependentValue.GetType().IsArray)
            {
                DependentValues = (IEnumerable<object>)dependentValue;
            }
            else
            {
                DependentValues = new[] { dependentValue };
            }

            convertedValues = dependentValue == null ? new string[0] : DependentValues.Select(x => x.ToString());
        }

        /// <summary>
        /// Gets the property to check for one of the dependant values.
        /// </summary>
        public string DependentProperty { get; }

        /// <summary>
        /// Gets the dependant value.
        /// </summary>
        public object DependentValue { get; }

        /// <summary>
        /// Gets the collection of values that will trigger this property to be required.
        /// </summary>
        public IEnumerable<object> DependentValues { get; }

        public string ValidatorKeyword => "x-validator-required-if";

        public Task PopulateAsync(JsonSchema4 schema, ApiOperationContext apiOperationContext)
        {
            schema.ExtensionData[ValidatorKeyword] = new Dictionary<string, object>
            {
                ["$data"] = $"1/{DependentProperty.Camelize()}",
                ["dependentValues"] = DependentValues,
            };

            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates the specified value.
        /// If the dependant property contains one of the values inside dependant values then value
        /// must not be null.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult"/> class.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var item = validationContext.ObjectInstance.GetType().GetProperty(DependentProperty).GetValue(validationContext.ObjectInstance, null);

            // We are required because the other item is null
            if (DependentValue == null && item == null && value != null)
            {
                return ValidationResult.Success;
            }

            if (item == null || !convertedValues.Contains(item.ToString()))
            {
                return ValidationResult.Success;
            }

            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), new[] { validationContext.DisplayName });
        }
    }
}
