using System.Collections.Generic;

namespace Blueprint
{
    /// <summary>
    /// A factory that can be used to consolidate the creation of <see cref="ApiException" /> and put the
    /// static values in an easily accessible location for tooling to read from, and for exception to
    /// be created from.
    /// </summary>
    /// <remarks>
    /// By using exception factories it is possible to for the API to be more self-describing as the
    /// factories can be public within an operation handler class and therefore can be
    /// read on API creation to populate <see cref="ResponseDescriptor" />s of the operation.
    /// </remarks>
    public class ApiExceptionFactory
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ApiException" /> class.
        /// </summary>
        /// <param name="title">The title of this exception, which <em>SHOULD NOT</em> change from occurrence to
        ///     occurrence.</param>
        /// <param name="type">The type of this exception, which <em>SHOULD NOT</em> change from occurrence to
        ///     occurrence, and is typically a URI that when followed gives more details of the problem.</param>
        /// <param name="httpStatus">The HTTP status code this exception is best represented by.</param>
        public ApiExceptionFactory(string title, string type, int httpStatus)
        {
            Title = title;
            Type = type;
            HttpStatus = httpStatus;
        }

        /// <summary>
        /// The HTTP status code([RFC7231], Section 6) generated by the origin server for this occurrence of the problem.
        /// </summary>
        public int HttpStatus { get; }

        /// <summary>
        /// A URI reference [RFC3986] that identifies the problem type. This specification encourages that, when
        /// dereferenced, it provide human-readable documentation for the problem type
        /// (e.g., using HTML [W3C.REC-html5-20141028]).  When this member is not present, its value is assumed to be
        /// "about:blank".
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// A short, human-readable summary of the problem type. It SHOULD NOT change from occurrence to occurrence
        /// of the problem, except for purposes of localization (e.g., using proactive content negotiation;
        /// see[RFC7231], Section 3.4).
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Creates a new <see cref="ApiException" /> using the details this factory was created with, in addition
        /// to the specified detail string.
        /// </summary>
        /// <param name="detail">The instance-specific error message.</param>
        /// <param name="extensionData">Extension details that will be serialised to the consumer, useful for storing additional
        /// properties that a client can use to make decisions when handling this exception.</param>
        /// <returns>A new <see cref="ApiException" />.</returns>
        public ApiException Create(string detail, IDictionary<string, object> extensionData = null)
        {
            return new ApiException(this.Title, this.Type, detail, this.HttpStatus)
            {
                Extensions = extensionData,
            };
        }
    }
}
