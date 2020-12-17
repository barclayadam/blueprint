using System;
using System.Threading.Tasks;

namespace Blueprint.Http.Formatters
{
    /// <summary>
    /// Reads an object from the request body.
    /// </summary>
    public abstract class BodyParser : IBodyParser
    {
        /// <summary>
        /// Gets the mutable collection of media type elements supported by
        /// this <see cref="BodyParser"/>.
        /// </summary>
        public MediaTypeCollection SupportedMediaTypes { get; } = new MediaTypeCollection();

        /// <inheritdoc />
        public virtual bool CanRead(BodyParserContext context)
        {
            if (this.SupportedMediaTypes.Count == 0)
            {
                throw new InvalidOperationException($"{this.GetType().FullName} does not specify any support media types. Ensure that {nameof(this.SupportedMediaTypes)} has been populated with at least one value");
            }

            if (!this.CanReadType(context.BodyType))
            {
                return false;
            }

            var contentType = context.HttpContext.Request.ContentType;
            if (string.IsNullOrEmpty(contentType))
            {
                return false;
            }

            // Confirm the request's content type is more specific than a media type this formatter supports e.g. OK if
            // client sent "text/plain" data and this formatter supports "text/*".
            return this.IsSubsetOfAnySupportedContentType(contentType);
        }

        /// <inheritdoc/>
        public abstract Task<object> ReadAsync(BodyParserContext context);

        private bool IsSubsetOfAnySupportedContentType(string contentType)
        {
            var parsedContentType = new MediaType(contentType);

            for (var i = 0; i < this.SupportedMediaTypes.Count; i++)
            {
                var supportedMediaType = new MediaType(this.SupportedMediaTypes[i]);
                if (parsedContentType.IsSubsetOf(supportedMediaType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether this <see cref="BodyParser"/> can deserialize an object of the given
        /// <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of object that will be read.</param>
        /// <returns><c>true</c> if the <paramref name="type"/> can be read, otherwise <c>false</c>.</returns>
        protected virtual bool CanReadType(Type type)
        {
            return true;
        }
    }
}
