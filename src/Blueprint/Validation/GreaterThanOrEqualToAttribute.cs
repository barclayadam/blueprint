﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Blueprint.Validation;

/// <summary>
/// Requires the property this attribute is assigned to, to be a number greater than or equal to the specified minimum value.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
public sealed class GreaterThanOrEqualToAttribute : ValidationAttribute
{
    private readonly int _minimumValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="GreaterThanOrEqualToAttribute"/> class.
    /// </summary>
    /// <param name="minimumValue">
    /// The minimum value allowed.
    /// </param>
    public GreaterThanOrEqualToAttribute(int minimumValue)
    {
        this._minimumValue = minimumValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GreaterThanOrEqualToAttribute"/> class.
    /// </summary>
    /// <param name="minimumValue">
    /// The minimum value allowed.
    /// </param>
    /// <param name="errorMessage">
    /// The error message.
    /// </param>
    public GreaterThanOrEqualToAttribute(int minimumValue, string errorMessage)
        : base(errorMessage)
    {
        this._minimumValue = minimumValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GreaterThanOrEqualToAttribute"/> class.
    /// </summary>
    /// <param name="minimumValue">
    /// The minimum value allowed.
    /// </param>
    /// <param name="errorMessageAccessor">
    /// The function that enables access to validation resources.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    /// <paramref name="errorMessageAccessor"/> is null.
    /// </exception>
    public GreaterThanOrEqualToAttribute(int minimumValue, Func<string> errorMessageAccessor)
        : base(errorMessageAccessor)
    {
        this._minimumValue = minimumValue;
    }

    /// <summary>
    /// Gets the minimum value for the property.
    /// </summary>
    public int MinimumValue => this._minimumValue;

    /// <summary>
    /// If the value is greater than that provided by the constructor, this returns true. Null objects also evaluate to true.
    /// </summary>
    /// <param name="value">
    /// The value to check.
    /// </param>
    /// <returns>
    /// True if the value is greater than the minimum value, else false.
    /// </returns>
    public override bool IsValid(object value)
    {
        if (value == null)
        {
            return true;
        }

        var number = double.Parse(value.ToString());
        return number >= this.MinimumValue;
    }
}