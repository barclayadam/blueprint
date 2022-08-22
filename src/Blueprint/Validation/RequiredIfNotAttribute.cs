using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Blueprint.Validation;

/// <summary>
/// Makes the property required if the 'dependant property' has a value that doesn't exists in the 'dependant value'.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RequiredIfNotAttribute : ValidationAttribute
{
    private const string RequiredFieldMessage = "The field {0} is required.";

    private readonly IEnumerable<string> _convertedValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
    /// </summary>
    /// <param name="dependantProperty">The dependant property.</param>
    /// <param name="dependantValue">The dependant value(s).</param>
    public RequiredIfNotAttribute(string dependantProperty, object dependantValue)
        : base(RequiredFieldMessage)
    {
        this.DependantProperty = dependantProperty;
        this.DependantValue = dependantValue;

        var isArray = dependantValue != null && dependantValue.GetType().IsArray;

        if (isArray)
        {
            this.DependantValues = (IEnumerable<object>)dependantValue;
        }
        else
        {
            this.DependantValues = new[] { dependantValue };
        }

        this._convertedValues = this.DependantValues.Select(x => x == null ? null : x.ToString()).Distinct().ToList();
    }

    /// <summary>
    /// Gets the dependant value.
    /// </summary>
    public object DependantValue { get; }

    /// <summary>
    /// Gets the property to check for one of the dependant values.
    /// </summary>
    public string DependantProperty { get; }

    /// <summary>
    /// Gets the collection of values that will trigger this property to be required.
    /// </summary>
    public IEnumerable<object> DependantValues { get; }

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
        var item = validationContext.ObjectInstance.GetType().GetProperty(this.DependantProperty).GetValue(validationContext.ObjectInstance, null);

        if (item == null || this._convertedValues.Contains(item.ToString()))
        {
            return ValidationResult.Success;
        }

        if (value != null && !string.IsNullOrEmpty(value.ToString()))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"Please fill in '{validationContext.DisplayName}'", new[] { validationContext.DisplayName });
    }
}