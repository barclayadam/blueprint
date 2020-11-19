using System;
using System.Diagnostics;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using Blueprint.Apm;
using Microsoft.Extensions.Configuration;

namespace Blueprint.Http
{
    /// <summary>
    /// An <see cref="IOperationResultExecutor{TResult}" /> for <see cref="UnhandledExceptionOperationResult" /> that will
    /// interrogate the <see cref="UnhandledExceptionOperationResult.Exception" /> to convert to the most appropriate
    /// HTTP response
    /// </summary>
    public class UnhandledExceptionOperationResultExecutor : IOperationResultExecutor<UnhandledExceptionOperationResult>
    {
        private readonly OkResultOperationExecutor _okResultOperationExecutor;
        private readonly bool _shouldExposeErrorMessage;

        /// <summary>
        /// Initialises a new instance of the <see cref="UnhandledExceptionOperationResultExecutor" /> class.
        /// </summary>
        /// <param name="configuration">The configuration of the application.</param>
        /// <param name="okResultOperationExecutor">The <see cref="OkResultOperationExecutor"/> writing is delegated to.</param>
        public UnhandledExceptionOperationResultExecutor(IConfiguration configuration, OkResultOperationExecutor okResultOperationExecutor)
        {
            this._okResultOperationExecutor = okResultOperationExecutor;
            this._shouldExposeErrorMessage = Convert.ToBoolean(configuration["Api:ExposeErrorMessage"] ?? "false");
        }

        /// <inheritdoc />
        public Task ExecuteAsync(ApiOperationContext context, UnhandledExceptionOperationResult result)
        {
            var httpContext = context.GetHttpContext();
            var problemDetails = this.ToProblemDetails(result.Exception);

            var traceId = context.ApmSpan?.TraceId;

            if (traceId != null)
            {
                problemDetails.AddExtension("traceId", traceId);
            }

            return this._okResultOperationExecutor.WriteContentAsync(
                httpContext,
                problemDetails.Status.Value,
                problemDetails);
        }

        private ProblemDetails ToProblemDetails(Exception exception)
        {
            switch (exception)
            {
                case ApiException apiException:
                    return new ProblemDetails
                    {
                        Status = apiException.HttpStatus,
                        Title = apiException.Title,
                        Type = apiException.Type,
                        Detail = apiException.Detail,
                        Extensions = apiException.Extensions,
                    };

                case SecurityException _:
                    return new ProblemDetails
                    {
                        Status = (int)HttpStatusCode.Unauthorized,
                        Type = "security_failure",
                        Title = exception.Message,
                    };

                case InvalidOperationException _:
                    return new ProblemDetails
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Type = "invalid_request",
                        Title = this._shouldExposeErrorMessage ? exception.Message : "There was a problem with the request",
                        Detail = this._shouldExposeErrorMessage ? exception.ToString() : null,
                    };

                default:
                    return new ProblemDetails
                    {
                        Status = (int)HttpStatusCode.InternalServerError,
                        Type = "unknown_error",
                        Title = this._shouldExposeErrorMessage ? exception.Message : "Something has gone wrong, please try again",
                        Detail = this._shouldExposeErrorMessage ? exception.ToString() : null,
                    };
            }
        }
    }
}
