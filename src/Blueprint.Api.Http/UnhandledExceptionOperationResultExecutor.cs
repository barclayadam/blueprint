using System;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using Blueprint.Api.Errors;
using Blueprint.Core.Errors;
using Microsoft.Extensions.Configuration;

namespace Blueprint.Api.Http
{
    /// <summary>
    /// An <see cref="IOperationResultExecutor{TResult}" /> for <see cref="UnhandledExceptionOperationResult" /> that will
    /// interrogate the <see cref="UnhandledExceptionOperationResult.Exception" /> to convert to the most appropriate
    /// HTTP response
    /// </summary>
    public class UnhandledExceptionOperationResultExecutor : IOperationResultExecutor<UnhandledExceptionOperationResult>
    {
        private readonly OkResultOperationExecutor okResultOperationExecutor;
        private readonly bool shouldExposeErrorMessage;

        /// <summary>
        /// Initialises a new instance of the <see cref="UnhandledExceptionOperationResultExecutor" /> class.
        /// </summary>
        /// <param name="configuration">The configuration of the application.</param>
        /// <param name="okResultOperationExecutor">The <see cref="OkResultOperationExecutor"/> writing is delegated to.</param>
        public UnhandledExceptionOperationResultExecutor(IConfiguration configuration, OkResultOperationExecutor okResultOperationExecutor)
        {
            this.okResultOperationExecutor = okResultOperationExecutor;
            shouldExposeErrorMessage = Convert.ToBoolean(configuration["Api.ExposeErrorMessage"] ?? "false");
        }

        /// <inheritdoc />
        public Task ExecuteAsync(ApiOperationContext context, UnhandledExceptionOperationResult result)
        {
            return okResultOperationExecutor.WriteContentAsync(
                context.GetHttpContext(),
                ToStatusCode(result.Exception),
                ToErrorResponse(result.Exception));
        }

        private static HttpStatusCode ToStatusCode(Exception exception)
        {
            switch (exception)
            {
                case ApiException apiException:
                    return apiException.HttpStatus;

                case SecurityException _:
                    return HttpStatusCode.Unauthorized;

                case InvalidOperationException _:
                    return HttpStatusCode.BadRequest;
            }

            return HttpStatusCode.InternalServerError;
        }

        private ErrorResponse ToErrorResponse(Exception exception)
        {
            return new ErrorResponse
            {
                Error = ToErrorResponseDetail(exception),
            };
        }

        private ErrorResponseDetail ToErrorResponseDetail(Exception e)
        {
            switch (e)
            {
                case IApiErrorDescriptor errorCodeProvider:
                    return new ErrorResponseDetail
                    {
                        Code = errorCodeProvider.ErrorCode,
                        Message = errorCodeProvider.ErrorMessage,
                    };

                case InvalidOperationException _:
                    return new ErrorResponseDetail
                    {
                        Code = "bad_request",
                        Message = e.Message,
                    };

                case SecurityException _:
                    return new ErrorResponseDetail
                    {
                        Code = "unauthenticated",
                        Message = e.Message,
                    };
            }

            return new ErrorResponseDetail
            {
                Code = "unknown_error",

                Message = shouldExposeErrorMessage ? e.ToString() : "Something has gone wrong, please try again",
            };
        }
    }
}
