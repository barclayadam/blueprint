using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Blueprint.Properties;
using Blueprint.Utilities;

namespace Blueprint.Validation;

/// <summary>
/// A validation expression that indicates the value must represent a value in the
/// past.
/// </summary>
[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "errorMessageAccessor: This belongs to the base class and cannot have a public accessor.")]
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class InPastAttribute : ValidationAttribute
{
    private readonly TemporalCheck _temporalCheckType;

    /// <summary>
    /// Initializes a new instance of the <see cref="InPastAttribute"/> class.
    /// </summary>
    /// <param name="temporalCheckType">The type of check to perform.</param>
    public InPastAttribute(TemporalCheck temporalCheckType)
    {
        this._temporalCheckType = temporalCheckType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InPastAttribute"/> class.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="temporalCheckType">The type of check to perform.</param>
    public InPastAttribute(string errorMessage, TemporalCheck temporalCheckType)
        : base(errorMessage)
    {
        this._temporalCheckType = temporalCheckType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InPastAttribute"/> class.
    /// </summary>
    /// <param name="errorMessageAccessor">The function that enables access to validation resources.</param>
    /// <param name="temporalCheckType">The type of check to perform.</param>
    /// <exception cref="System.ArgumentNullException"><paramref name="errorMessageAccessor"/> is null.</exception>
    public InPastAttribute(Func<string> errorMessageAccessor, TemporalCheck temporalCheckType)
        : base(errorMessageAccessor)
    {
        this._temporalCheckType = temporalCheckType;
    }

    /// <summary>
    /// Evaluates if the provided object is in the past or not.
    /// </summary>
    /// <param name="value">
    /// The date to evaluate.
    /// </param>
    /// <returns>
    /// True if the date is in the future.
    /// </returns>
    public override bool IsValid(object value)
    {
        if (value == null)
        {
            return true;
        }

        if (value is DateTimeOffset o)
        {
            return o.IsInPast(this._temporalCheckType);
        }

        if (value is DateTime d)
        {
            return d.IsInPast(this._temporalCheckType);
        }

        if (value is string s)
        {
            return DateTime.TryParse(s, out var valueAsDateTime) && valueAsDateTime.IsInPast(this._temporalCheckType);
        }

        throw new InvalidOperationException(Resources.InFutureAttribute_IsValid_NotDateTime_Message);
    }
}