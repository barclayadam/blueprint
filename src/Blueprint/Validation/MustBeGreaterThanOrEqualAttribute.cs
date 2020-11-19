using System;
using System.ComponentModel.DataAnnotations;

namespace Blueprint.Validation
{
    /// <summary>
    /// Ensures the property has a greater value than the 'dependant property'.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MustBeGreaterThanOrEqualAttribute : ValidationAttribute
    {
        private const string RequiredIfFieldMessage = "The {0} field must be greater than or equal to";

        public MustBeGreaterThanOrEqualAttribute(string dependentProperty)
            : base(RequiredIfFieldMessage + dependentProperty)
        {
            this.DependentProperty = dependentProperty;
        }

        /// <summary>
        /// Gets the property to check for one of the dependant values.
        /// </summary>
        public string DependentProperty { get; }

        protected override ValidationResult IsValid(object maxValue, ValidationContext validationContext)
        {
            var property = validationContext.ObjectInstance.GetType().GetProperty(this.DependentProperty);
            if (property == null)
            {
                throw new InvalidOperationException($"{this.DependentProperty} was null!");
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
                throw new InvalidOperationException($"{this.DependentProperty} is not IComparable");
            }

            return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName), new[] { validationContext.DisplayName });
        }
    }
}
