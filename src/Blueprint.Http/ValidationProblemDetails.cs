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
            this.Title = "One or more validation errors occurred";
            this.Type = "validation_failed";
            this.Status = 422;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ValidationProblemDetails" /> class
        /// with a dictionary mapping properties to a list of errors.
        /// </summary>
        /// <param name="errors">The errors to represent.</param>
        public ValidationProblemDetails(IReadOnlyDictionary<string, IEnumerable<string>> errors)
            : this()
        {
            this.Errors = errors;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ValidationProblemDetails" /> class
        /// from an instance of <see cref="ValidationFailures" />.
        /// </summary>
        /// <param name="errors">The errors to represent.</param>
        public ValidationProblemDetails(ValidationFailures errors)
            : this()
        {
            this.Errors = errors.AsDictionary();
        }

        /// <summary>
        /// Gets the validation errors associated with this instance of <see cref="ValidationProblemDetails"/>.
        /// </summary>
        [JsonPropertyName("errors")]
        public IReadOnlyDictionary<string, IEnumerable<string>> Errors { get; }
    }
}
