using System;
using System.Diagnostics.CodeAnalysis;
using Blueprint.Utilities;

namespace Blueprint.Validation;

/// <summary>
/// Provides a validator which will check for a valid phone number, using the pre-built regular expressions found
/// in the RegularExpressions class.
/// </summary>
[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "errorMessageAccessor is defined in base class, not accessible")]
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
public sealed class PhoneNumberAttribute : RegexAttribute
{
    /// <summary>
    /// Initializes a new instance of the PhoneNumberAttribute class.
    /// </summary>
    public PhoneNumberAttribute()
        : base(CommonRegularExpressions.PhoneNumberOnly, "Please enter a valid phone number")
    {
    }

    /// <summary>
    /// Initializes a new instance of the PhoneNumberAttribute class.
    /// </summary>
    /// <param name="errorMessage">
    /// The error message to be shown on validation failure.
    /// </param>
    public PhoneNumberAttribute(string errorMessage)
        : base(CommonRegularExpressions.PhoneNumberOnly, errorMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the PhoneNumberAttribute class.
    /// </summary>
    /// <param name="errorMessageAccessor">
    /// A function which will return the error message
    /// to be shown on failure.
    /// </param>
    public PhoneNumberAttribute(Func<string> errorMessageAccessor)
        : base(CommonRegularExpressions.PhoneNumberOnly, errorMessageAccessor)
    {
    }
}