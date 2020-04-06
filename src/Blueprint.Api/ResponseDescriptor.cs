using System;
using Blueprint.Core;

namespace Blueprint.Api
{
    /// <summary>
    /// Describes a single response that could be generated by an <see cref="IApiOperation" />.
    /// </summary>
    public class ResponseDescriptor
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ResponseDescriptor" /> class.
        /// </summary>
        /// <param name="type">The type of response.</param>
        public ResponseDescriptor(Type type)
        {
            Guard.NotNull(nameof(type), type);

            Type = type;
        }

        /// <summary>
        /// Gets the type of response, which will often be derived from <see cref="ResourceEvent" /> for
        /// commands or <see cref="ApiResource" /> for queries.
        /// </summary>
        public Type Type { get; }
    }
}