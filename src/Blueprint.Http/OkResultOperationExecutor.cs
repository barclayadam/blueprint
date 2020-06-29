using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Blueprint.Http.Formatters;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Http
{
    /// <summary>
    /// An <see cref="IOperationResultExecutor{TResult}" /> for <see cref="OkResult" /> that will find a
    /// registered <see cref="IOperationResultOutputFormatter" /> to use for writing the <see cref="OkResult.Content" />
    /// property to the HTTP response of the request.
    /// </summary>
    public class OkResultOperationExecutor : IOperationResultExecutor<OkResult>
    {
        private readonly IEnumerable<IOperationResultOutputFormatter> formatters;
        private readonly IOperationResultOutputFormatter defaultFormatter;

        /// <summary>
        /// Initialises a new instance of the <see cref="OkResultOperationExecutor" /> class.
        /// </summary>
        /// <param name="formatters">A list of registered <see cref="IOperationResultOutputFormatter"/>s.</param>
        public OkResultOperationExecutor(IEnumerable<IOperationResultOutputFormatter> formatters)
        {
            this.formatters = formatters.ToList();

            defaultFormatter = this.formatters.Single(f => f is JsonOperationResultOutputFormatter);
        }

        /// <inheritdoc />
        public Task ExecuteAsync(ApiOperationContext context, OkResult result)
        {
            var httpContext = context.GetHttpContext();

            return WriteContentAsync(httpContext, (int)HttpStatusCode.OK, result.Content);
        }

        /// <summary>
        /// Writes the given result to the response stream, in addition to setting the status code.
        /// </summary>
        /// <param name="httpContext">The HTTP context containing request to determine format + response to write output to.</param>
        /// <param name="statusCode">The status code to set.</param>
        /// <param name="result">The result to render.</param>
        /// <returns>A <see cref="Task"/> representing this async operation.</returns>
        public async Task WriteContentAsync(HttpContext httpContext, int statusCode, object result)
        {
            var httpRequest = httpContext.Request;

            var requestedFormat = GetRequestedFormat(httpRequest);
            var formatter = requestedFormat != null ?
                GetFormatter(httpRequest, requestedFormat) :
                defaultFormatter;

            var httpResponse = httpContext.Response;

            httpResponse.StatusCode = statusCode;
            await formatter.WriteAsync(httpResponse, requestedFormat, result);
        }

        private static string GetRequestedFormat(HttpRequest request)
        {
            // Bail early if no query string to avoid allocations on getting the query string
            // values (GetQueryNameValuePairs)
            if (request == null || request.QueryString.HasValue == false)
            {
                return null;
            }

            if (request.Query.TryGetValue("format", out var format))
            {
                return format[0];
            }

            return null;
        }

        private IOperationResultOutputFormatter GetFormatter(HttpRequest request, string requestedFormat)
        {
            // PERF: Do not use LINQ to avoid allocating closure
            foreach (var formatter in formatters)
            {
                if (formatter.IsSupported(request, requestedFormat))
                {
                    return formatter;
                }
            }

            throw new ApiException(
                "The requested format is not supported",
                "unsupported_format",
                $"{requestedFormat} is not supported",
                (int)HttpStatusCode.UnsupportedMediaType);
        }
    }
}
