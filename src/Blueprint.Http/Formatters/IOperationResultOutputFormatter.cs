using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Http.Formatters
{
    public interface IOperationResultOutputFormatter
    {
        /// <summary>
        /// Indicates whether this formatter supports the given request + format combination.
        /// </summary>
        /// <remarks>
        /// The format is an override that has been specified and should take precedence over the
        /// <see cref="HttpRequest" />, but if not specified the request can be used to look at
        /// the Accept header.
        /// </remarks>
        /// <param name="request">The HTTP request.</param>
        /// <param name="format">The specified format, may be null or empty.</param>
        /// <returns>Whether this formatter is supported for the given request and format.</returns>
        bool IsSupported(HttpRequest request, string format);

        Task WriteAsync(HttpResponse httpResponse, string format, object result);
    }
}
