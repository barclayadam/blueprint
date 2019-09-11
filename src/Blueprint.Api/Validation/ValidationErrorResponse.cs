using System.Collections.Generic;
using Blueprint.Core.Errors;

namespace Blueprint.Api.Validation
{
    public class ValidationErrorResponse : ErrorResponse
    {
        private static readonly ErrorResponseDetail DefaultErrorResponseDetail = new ErrorResponseDetail
        {
            Code = "validation_failed",
            Message = "Message validation has failed. See Errors property"
        };

        public ValidationErrorResponse(Dictionary<string, IEnumerable<string>> errors)
        {
            Error = DefaultErrorResponseDetail;
            Errors = errors;
        }

        public ValidationErrorResponse(ValidationFailures errors)
        {
            Error = DefaultErrorResponseDetail;
            Errors = errors.AsDictionary();
        }

        public Dictionary<string, IEnumerable<string>> Errors { get; set; }
    }
}
