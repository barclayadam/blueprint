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
        /// Initializes a new instance of <see cref="ValidationProblemDetails"/>.
        /// </summary>
        internal ValidationProblemDetails()
        {
            Title = "One or more validation errors occurred";
            Status = 422;
        }

        public ValidationProblemDetails(IDictionary<string, IEnumerable<string>> errors)
            : this()
        {
            Errors = errors;
        }

        public ValidationProblemDetails(ValidationFailures errors)
            : this()
        {
            Errors = errors.AsDictionary();
        }

        /// <summary>
        /// Gets the validation errors associated with this instance of <see cref="ValidationProblemDetails"/>.
        /// </summary>
        [JsonPropertyName("errors")]
        public IDictionary<string, IEnumerable<string>> Errors { get; } = new Dictionary<string, IEnumerable<string>>(StringComparer.Ordinal);
    }
}
