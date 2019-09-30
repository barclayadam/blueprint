using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Blueprint.Core.Utilities;

namespace Blueprint.Api.Validation
{
    public enum UrlProtocol
    {
        NotRequired,
        Required,
    }

    /// <summary>
    /// Provides a validator which will check for a valid URL, using the pre-built regular expressions found
    /// in the RegularExpressions class.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "errorMessageAccessor is defined in base class, not accessible")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class UrlAttribute : RegexAttribute
    {
        /// <summary>
        /// Initializes a new instance of the UrlAttribute class.
        /// </summary>
        /// <param name="urlProtocol">Determines how to check the incoming URL.</param>
        public UrlAttribute(UrlProtocol urlProtocol = UrlProtocol.NotRequired)
            : base(GetUrlProtocolRegexVariant(urlProtocol), GetUrlMessage(urlProtocol))
        {
        }

        /// <summary>
        /// Initializes a new instance of the UrlAttribute class.
        /// </summary>
        /// <param name="errorMessage">
        /// The error message to be shown on validation failure.
        /// </param>
        /// <param name="urlProtocol">
        /// The format to validate.
        /// </param>
        public UrlAttribute(string errorMessage, UrlProtocol urlProtocol = UrlProtocol.NotRequired)
            : base(GetUrlProtocolRegexVariant(urlProtocol), errorMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the UrlAttribute class.
        /// </summary>
        /// <param name="errorMessageAccessor">
        /// A function which will return the error message
        /// to be shown on failure.
        /// </param>
        /// <param name="urlProtocol">
        /// The format to validate.
        /// </param>
        public UrlAttribute(Func<string> errorMessageAccessor, UrlProtocol urlProtocol = UrlProtocol.NotRequired)
            : base(GetUrlProtocolRegexVariant(urlProtocol), errorMessageAccessor)
        {
        }

        private static Regex GetUrlProtocolRegexVariant(UrlProtocol urlProtocol)
        {
            return urlProtocol == UrlProtocol.NotRequired ?
                CommonRegularExpressions.UrlOnly :
                CommonRegularExpressions.UrlWithProtocol;
        }

        private static string GetUrlMessage(UrlProtocol urlProtocol)
        {
            return urlProtocol == UrlProtocol.NotRequired ?
                "Please enter a valid url" :
                "Please enter a fully qualified url";
        }
    }
}
