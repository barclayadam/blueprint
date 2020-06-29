using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Blueprint.Validation
{
    /// <summary>
    /// Requires that a string is of sufficent complexity to be classed as a valid password.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "errorMessageAccessor belongs to the base class, thus cannot have a public accesso.r")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class PasswordAttribute : RegexAttribute
    {
        private const string DefaultMessage = "A password must contain at least: 1 lower case letter, 1 upper case letter, 1 number, 1 special character and be at least 8 characters long.";

        /// <summary>
        /// A regular expression that matches any string that contains at least
        ///     +one lower case letter,
        ///     +one upper case letter,
        ///     +one number and one character that isn't a letter or number,
        ///     +if the string is 8 characters or more.
        /// </summary>
        private static readonly Regex StrongPassword =
                new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the PasswordAttribute class.
        /// </summary>
        public PasswordAttribute()
            : base(StrongPassword, DefaultMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PasswordAttribute class.
        /// </summary>
        /// <param name="errorMessage">
        /// The error message to be shown on validation failure.
        /// </param>
        public PasswordAttribute(string errorMessage)
            : base(StrongPassword, errorMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PasswordAttribute class.
        /// </summary>
        /// <param name="errorMessage">The error message to be shown on validation failure.</param>
        /// <param name="appendInvalidPasswordDescription">Determines whether the invalid password description should be appended to the error message.</param>
        public PasswordAttribute(string errorMessage, bool appendInvalidPasswordDescription)
            : base(StrongPassword, appendInvalidPasswordDescription ? errorMessage + " " + DefaultMessage : errorMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PasswordAttribute class.
        /// </summary>
        /// <param name="errorMessageAccessor">
        /// A function which will return the error message
        /// to be shown on failure.
        /// </param>
        public PasswordAttribute(Func<string> errorMessageAccessor)
            : base(StrongPassword, errorMessageAccessor)
        {
        }
    }
}
