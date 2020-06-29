using System;
using System.ComponentModel.DataAnnotations;

namespace Blueprint.Validation
{
    /// <summary>
    /// Makes the property required if the 'dependant property' has a value that exists in the 'dependant value'.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class GreaterThanPropertyAttribute : ValidationAttribute
    {
        private const string GreaterThanPropertyFieldMessage = "The field {0} is required.";

        /// <summary>
        /// Initializes a new instance of the <see cref="GreaterThanPropertyAttribute"/> class.
        /// </summary>
        /// <param name="dependantProperty">The dependant property.</param>
        public GreaterThanPropertyAttribute(string dependantProperty)
            : base(GreaterThanPropertyFieldMessage)
        {
            DependantProperty = dependantProperty;
        }

        /// <summary>
        /// Gets the property to check for one of the dependant values.
        /// </summary>
        public string DependantProperty { get; }

        /// <summary>
        /// Validates the specified value; if the dependant property contains one of the values inside dependant values then value
        /// must not be null.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult"/> class.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var item = validationContext.ObjectInstance.GetType().GetProperty(DependantProperty).GetValue(validationContext.ObjectInstance, null);

            if (item == null)
            {
                return ValidationResult.Success;
            }

            if (value.GetType() != item.GetType())
            {
                object[] args = new[] {value.GetType()};
                throw new InvalidOperationException(string.Format("The dependent property you supply must also be of type '{0}'.", args));
            }

            var comparableValue = value as IComparable;

            if (comparableValue == null)
            {
                object[] args = new[] {validationContext.DisplayName};
                throw new InvalidOperationException(string.Format("The property type of '{0}' is not comparable.", args));
            }

            return comparableValue.CompareTo(item) > 0 ? ValidationResult.Success : new ValidationResult(FormatErrorMessage(validationContext.DisplayName), new[] { validationContext.DisplayName });
        }
    }
}
