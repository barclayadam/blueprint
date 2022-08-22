using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Blueprint.Validation;

/// <summary>
/// Makes the property required if the 'dependant property' has a value that exists in the 'dependant value'.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RequiredIfAttribute : ValidationAttribute
{
    private const string RequiredIfFieldMessage = "The {0} field is required";

    private readonly IEnumerable<string> _convertedValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
    /// </summary>
    /// <param name="dependentProperty">The dependant property.</param>
    /// <param name="dependentValue">The dependant value(s).</param>
    public RequiredIfAttribute(string dependentProperty, object dependentValue)
        : base(RequiredIfFieldMessage + $@" if {dependentProperty} field is {dependentValue}.")
    {
        this.DependentProperty = dependentProperty;
        this.DependentValue = dependentValue;

        if (dependentValue != null && dependentValue.GetType().IsArray)
        {
            this.DependentValues = (IEnumerable<object>)dependentValue;
        }
        else
        {
            this.DependentValues = new[] { dependentValue };
        }

        this._convertedValues = dependentValue == null ? new string[0] : this.DependentValues.Select(x => x.ToString());
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
        var item = validationContext.ObjectInstance.GetType().GetProperty(this.DependentProperty).GetValue(validationContext.ObjectInstance, null);

        // We are required because the other item is null
        if (this.DependentValue == null && item == null && value != null)
        {
            return ValidationResult.Success;
        }

        if (item == null || !this._convertedValues.Contains(item.ToString()))
        {
            return ValidationResult.Success;
        }

        if (value != null && !string.IsNullOrEmpty(value.ToString()))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName), new[] { validationContext.DisplayName });
    }
}