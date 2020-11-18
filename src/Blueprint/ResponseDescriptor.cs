using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blueprint
{
    /// <summary>
    /// Describes a single response that could be generated by an operation.
    /// </summary>
    public class ResponseDescriptor
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ResponseDescriptor" /> class.
        /// </summary>
        /// <param name="type">The type of response.</param>
        /// <param name="httpStatus">The HTTP status of this response.</param>
        /// <param name="description">A description of this response.</param>
        /// <param name="metadata">A set of extra metadata attributes for this response, useful for conveying
        /// extra information from, for example, the XML tags describing errors, that could be further used in
        /// OpenAPI generation.</param>
        public ResponseDescriptor(
            Type type,
            int httpStatus,
            string description,
            [CanBeNull] Dictionary<string, string> metadata = null)
        {
            Guard.NotNull(nameof(type), type);

            Type = type;
            HttpStatus = httpStatus;
            Description = description;
            Metadata = metadata;
        }

        /// <summary>
        /// Gets the type of response.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The HTTP status of this response. Note that although we use HTTP status codes here, we are
        /// not explicitly tied to HTTP, we just reuse semantics of the codes as they are fairly broad
        /// and have well-defined meanings.
        /// </summary>
        public int HttpStatus { get; }

        /// <summary>
        /// A description of this response.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// A set of extra metadata attributes for this response, useful for conveying
        /// extra information from, for example, the XML tags describing errors, that could be further used in
        /// OpenAPI generation.
        /// </summary>
        [CanBeNull]
        public Dictionary<string, string> Metadata { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Type}: {HttpStatus}";
        }
    }
}
