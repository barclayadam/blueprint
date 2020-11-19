using System.Net;
using System.Threading.Tasks;
using Blueprint.Http.Formatters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Blueprint.Http
{
    /// <summary>
    /// An <see cref="IOperationResultExecutor{TResult}" /> for <see cref="OkResult" /> that will find a
    /// registered <see cref="IOperationResultOutputFormatter" /> to use for writing the <see cref="OkResult.Content" />
    /// property to the HTTP response of the request.
    /// </summary>
    public class OkResultOperationExecutor : IOperationResultExecutor<OkResult>
    {
        private readonly IOutputFormatterSelector _outputFormatterSelector;
        private readonly ILogger<OkResultOperationExecutor> _logger;

        /// <summary>
        /// Initialises a new instance of the <see cref="OkResultOperationExecutor" /> class.
        /// </summary>
        /// <param name="outputFormatterSelector">A output formatter selector.</param>
        /// <param name="logger">A logger.</param>
        public OkResultOperationExecutor(IOutputFormatterSelector outputFormatterSelector, ILogger<OkResultOperationExecutor> logger)
        {
            this._outputFormatterSelector = outputFormatterSelector;
            this._logger = logger;
        }

        /// <inheritdoc />
        public Task ExecuteAsync(ApiOperationContext context, OkResult result)
        {
            var httpContext = context.GetHttpContext();

            return this.WriteContentAsync(httpContext, (int)HttpStatusCode.OK, result.Content);
        }

        /// <summary>
        /// Writes the given result to the response stream, in addition to setting the status code.
        /// </summary>
        /// <param name="httpContext">The HTTP context containing request to determine format + response to write output to.</param>
        /// <param name="statusCode">The status code to set.</param>
        /// <param name="result">The result to render.</param>
        /// <returns>A <see cref="Task"/> representing this async operation.</returns>
        public Task WriteContentAsync(HttpContext httpContext, int statusCode, object result)
        {
            var httpResponse = httpContext.Response;

            var context = new OutputFormatterCanWriteContext(httpContext, result);
            var formatter = this._outputFormatterSelector.SelectFormatter(context);

            if (formatter == null)
            {
                this._logger.LogDebug("Could not determine output formatter to use. Returning 406 Not Acceptable");

                httpResponse.StatusCode = StatusCodes.Status406NotAcceptable;

                return Task.CompletedTask;
            }

            httpResponse.StatusCode = statusCode;

            return formatter.WriteAsync(context);
        }
    }
}
