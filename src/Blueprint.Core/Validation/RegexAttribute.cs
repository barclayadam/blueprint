using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Blueprint.Core.Api;

namespace Blueprint.Core.Validation
{
    using NJsonSchema;

    using OpenApi;

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
    public abstract class RegexAttribute : ValidationAttribute, IOpenApiValidationAttribute
    {
        private readonly Regex regex;

        /// <summary>
        /// Initializes a new instance of the RegexAttribute class.
        /// </summary>
        /// <param name="regex">The regular expression to check.</param>
        protected RegexAttribute(Regex regex)
        {
            this.regex = regex;
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
            this.regex = regex;
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
            this.regex = regex;
        }

        /// <summary>
        /// Gets the regular expression used by this validation attribute.
        /// </summary>
        public Regex Regex => this.regex;

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

            var match = Regex.Match(input);

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
                ErrorMessageString,
                new object[] { name, regex.ToString() });
        }

        public string ValidatorKeyword => "pattern";

        public virtual async ValueTask PopulateAsync(JsonSchema4 schema, ApiOperationContext apiOperationContext)
        {
            if (regex.Options.HasFlag(RegexOptions.IgnoreCase))
            {
                throw new Exception("Ignore case flag is not supported.");
            }

            schema.Pattern = regex.ToString();
        }
    }
}
