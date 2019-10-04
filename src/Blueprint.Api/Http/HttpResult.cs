using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace Blueprint.Api.Http
{
    /// <summary>
    /// Provides a base class for all HTTP-related operation results that provide basics like header and status
    /// code management that all results could take advantage of.
    /// </summary>
    public abstract class HttpResult : OperationResult
    {
        private Dictionary<string, StringValues> headers;

        protected HttpResult(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the status code that will be set for the HTTP response this operation result represents.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets a dictionary that can be populated with custom headers to set on the response when this
        /// result is executed.
        /// </summary>
        public Dictionary<string, StringValues> Headers
        {
            get
            {
                return headers ?? (headers = new Dictionary<string, StringValues>());
            }
        }

        /// <summary>
        /// Sets the HTTP status code and adds any custom headers that have been added to <see cref="Headers" />.
        /// </summary>
        /// <param name="context">The context of the operation this result is for.</param>
        /// <returns>A <see cref="Task" /> representing the execution of this method.</returns>
        public override Task ExecuteAsync(ApiOperationContext context)
        {
            context.Response.StatusCode = (int)StatusCode;

            if (Headers != null)
            {
                foreach (var h in Headers)
                {
                    context.Response.Headers.Add(h);
                }
            }

            return Task.CompletedTask;
        }
    }
}