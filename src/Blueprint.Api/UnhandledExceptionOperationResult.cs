using System;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using Blueprint.Api.Errors;
using Blueprint.Api.Http;
using Blueprint.Core.Errors;
using Blueprint.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api
{
    public class UnhandledExceptionOperationResult : OkResult<ErrorResponse>
    {
        public UnhandledExceptionOperationResult(Exception exception) : base(ToStatusCode(exception), ToErrorResponse(exception))
        {
            Exception = exception;
        }

        public Exception Exception { get; }

        public override Task ExecuteAsync(ApiOperationContext context)
        {
            var configuration = context.ServiceProvider.GetService<IConfiguration>();
            var shouldExpose = Convert.ToBoolean(configuration["Api.ExposeErrorMessage"] ?? "true");

            if (!shouldExpose)
            {
                Content.Error.Message = "Something has gone wrong, please try again";
            }

            return base.ExecuteAsync(context);
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

        private static ErrorResponse ToErrorResponse(Exception exception)
        {
            return new ErrorResponse
            {
                Error = GetErrorResponse(exception),
            };
        }

        private static ErrorResponseDetail GetErrorResponse(Exception e)
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

                Message = e.ToString(),
            };
        }
    }
}
