using System;
using System.Diagnostics.CodeAnalysis;
using Blueprint.Core.Utilities;

namespace Blueprint.Core.Validation
{
    /// <summary>
    /// Provides a validator which will check for a valid email address, using the pre-built regular expressions found
    /// in the RegularExpressions class.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments",
            Justification = "errorMessageAccessor is defined in base class, not accessible")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class EmailAddressAttribute : RegexAttribute
    {
        /// <summary>
        /// Initializes a new instance of the EmailAddressAttribute class.
        /// </summary>
        public EmailAddressAttribute() : base(CommonRegularExpressions.EmailOnly, "Please enter a valid email address")
        {
        }

        /// <summary>
        /// Initializes a new instance of the EmailAddressAttribute class.
        /// </summary>
        /// <param name="errorMessage">
        /// The error message to be shown on validation failure.
        /// </param>
        public EmailAddressAttribute(string errorMessage)
            : base(CommonRegularExpressions.EmailOnly, errorMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the EmailAddressAttribute class.
        /// </summary>
        /// <param name="errorMessageAccessor">
        /// A function which will return the error message
        /// to be shown on failure.
        /// </param>
        public EmailAddressAttribute(Func<string> errorMessageAccessor)
            : base(CommonRegularExpressions.EmailOnly, errorMessageAccessor)
        {
        }
    }
}