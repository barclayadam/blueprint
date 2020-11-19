﻿using System;
using System.Diagnostics.CodeAnalysis;
using Blueprint.Utilities;

namespace Blueprint.Validation
{
    /// <summary>
    /// Provides a validator which will check for a valid UK postcode, using the pre-built regular expressions found
    /// in the RegularExpressions class.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "errorMessageAccessor is defined in base class, not accessible")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class UkPostcodeAttribute : RegexAttribute
    {
        /// <summary>
        /// Initializes a new instance of the UKPostcodeAttribute class.
        /// </summary>
        public UkPostcodeAttribute() : base(CommonRegularExpressions.UkPostcodeOnly)
        {
        }

        /// <summary>
        /// Initializes a new instance of the UKPostcodeAttribute class.
        /// </summary>
        /// <param name="errorMessage">
        /// The error message to be shown on validation failure.
        /// </param>
        public UkPostcodeAttribute(string errorMessage)
            : base(CommonRegularExpressions.UkPostcodeOnly, errorMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the UKPostcodeAttribute class.
        /// </summary>
        /// <param name="errorMessageAccessor">
        /// A function which will return the error message to be shown on failure.
        /// </param>
        public UkPostcodeAttribute(Func<string> errorMessageAccessor)
            : base(CommonRegularExpressions.UkPostcodeOnly, errorMessageAccessor)
        {
        }
    }
}
