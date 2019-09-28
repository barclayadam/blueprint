using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Blueprint.Core;

namespace Blueprint.Api.Validation
{
    /// <summary>
    /// An exception that will be thrown in the case of validation failures.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Only want ValidationException to be used in the specific way supported by the constructor.")]
    public class ValidationException : Exception
    {
        public ValidationException(string formLevelMessage)
            : this(formLevelMessage, new Dictionary<string, string> { [ValidationFailures.FormLevelPropertyName] = formLevelMessage })
        {
            Guard.NotNullOrEmpty(nameof(formLevelMessage), formLevelMessage);
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class for a single
        /// property being invalid as a utility to avoid a lot of boilerplate.
        /// </summary>
        /// <param name="property">The property that failure applies to.</param>
        /// <param name="message">The message of the exception.</param>
        public ValidationException(string property, string message)
            : this(message, new Dictionary<string, string> { [property] = message })
        {
            Guard.NotNullOrEmpty(nameof(message), message);
            Guard.NotNullOrEmpty(nameof(property), property);
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class.
        /// </summary>
        /// <param name="overallFailureMessage">The message of the exception.</param>
        /// <param name="validationResults">The validation failures.</param>
        public ValidationException(string overallFailureMessage, Dictionary<string, string> validationResults)
                : base(overallFailureMessage)
        {
            Guard.NotNull(nameof(validationResults), validationResults);

            // TODO: Is there a specialist 1-item collection we could use for performance?
            ValidationResults = validationResults.ToDictionary(k => k.Key, v => (IEnumerable<string>) new [] { v.Value });
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class.
        /// </summary>
        /// <param name="overallFailureMessage">The message of the exception.</param>
        /// <param name="validationResults">The validation failures.</param>
        public ValidationException(string overallFailureMessage, Dictionary<string, IEnumerable<string>> validationResults)
            : base(overallFailureMessage)
        {
            Guard.NotNull(nameof(validationResults), validationResults);

            ValidationResults = validationResults;
        }

        /// <summary>
        /// Gets the validation failures that have caused this validation exception.
        /// </summary>
        public Dictionary<string, IEnumerable<string>> ValidationResults { get; }
    }
}