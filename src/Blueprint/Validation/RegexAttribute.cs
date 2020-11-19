using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Blueprint.Validation
{
    /// <summary>
    /// An attribute that is designed to be subclassed when creating a custom validation attribute that
    /// uses regular expressions.
    /// </summary>
    /// <remarks>
    /// The standard <see cref="RegularExpressionAttribute" /> only allows passing in a string pattern, which
    /// limits usefulness slightly as <see cref="RegexOptions"/> cannot be passed down, forcing the use of
    /// 'inline' modifiers which are not compatible with JavaScript and therefore deserialising those patterns
    /// causes page failures.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public abstract class RegexAttribute : ValidationAttribute
    {
        private readonly Regex _regex;

        /// <summary>
        /// Initializes a new instance of the RegexAttribute class.
        /// </summary>
        /// <param name="regex">The regular expression to check.</param>
        protected RegexAttribute(Regex regex)
        {
            this._regex = regex;
        }

        /// <summary>
        /// Initializes a new instance of the RegexAttribute class.
        /// </summary>
        /// <param name="regex">The regular expression to check.</param>
        /// <param name="errorMessage">
        /// The error message to be shown on validation failure.
        /// </param>
        protected RegexAttribute(Regex regex, string errorMessage) : base(errorMessage)
        {
            this._regex = regex;
        }

        /// <summary>
        /// Initializes a new instance of the RegexAttribute class.
        /// </summary>
        /// <param name="regex">The regular expression to check.</param>
        /// <param name="errorMessageAccessor">
        /// A function which will return the error message to be shown on failure.
        /// </param>
        protected RegexAttribute(Regex regex, Func<string> errorMessageAccessor) : base(errorMessageAccessor)
        {
            this._regex = regex;
        }

        /// <summary>
        /// Gets the regular expression used by this validation attribute.
        /// </summary>
        public Regex Regex => this._regex;

        /// <summary>
        /// Determines whether or not the specified value is valid, which means that is matches
        /// fully the regular expression this attribute represents when converted to a string.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether the value is valid according to the regular expression of this attribute.</returns>
        public override bool IsValid(object value)
        {
            var input = Convert.ToString(value, CultureInfo.CurrentCulture);

            if (string.IsNullOrEmpty(input))
            {
                return true;
            }

            var match = this.Regex.Match(input);

            if (match.Success && match.Index == 0)
            {
                return match.Length == input.Length;
            }

            return false;
        }

        /// <summary>
        /// Formats the error message for this attribute, inserting the name and pattern of this
        /// regular expression to be placed into the message.
        /// </summary>
        /// <param name="name">The name of the property this attribute has been applied to.</param>
        /// <returns>A formatted error message.</returns>
        public override string FormatErrorMessage(string name)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                this.ErrorMessageString,
                name,
                this._regex.ToString());
        }
    }
}
