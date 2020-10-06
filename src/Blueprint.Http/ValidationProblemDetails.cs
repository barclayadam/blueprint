using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Blueprint.Validation;

namespace Blueprint.Http
{
    /// <summary>
    /// A <see cref="ProblemDetails"/> for validation errors.
    /// </summary>
    [JsonConverter(typeof(ValidationProblemDetailsJsonConverter))]
    public class ValidationProblemDetails : ProblemDetails
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ValidationProblemDetails" /> class
        /// with a dictionary mapping properties to a list of errors.
        /// </summary>
        /// <param name="errors">The errors to represent.</param>
        public ValidationProblemDetails(IDictionary<string, IEnumerable<string>> errors)
            : this()
        {
            Errors = errors;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ValidationProblemDetails" /> class
        /// from an instance of <see cref="ValidationFailures" />.
        /// </summary>
        /// <param name="errors">The errors to represent.</param>
        public ValidationProblemDetails(ValidationFailures errors)
            : this()
        {
            Errors = errors.AsDictionary();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ValidationProblemDetails"/>.
        /// </summary>
        internal ValidationProblemDetails()
        {
            Title = "One or more validation errors occurred";
            Type = "validation_failed";
            Status = 422;
        }

        /// <summary>
        /// Gets the validation errors associated with this instance of <see cref="ValidationProblemDetails"/>.
        /// </summary>
        [JsonPropertyName("errors")]
        public IDictionary<string, IEnumerable<string>> Errors { get; } = new Dictionary<string, IEnumerable<string>>(StringComparer.Ordinal);
    }
}
