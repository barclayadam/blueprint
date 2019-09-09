using System.Collections.Generic;
using System.Net;

using Blueprint.Core.Errors;
using Blueprint.Core.Validation;

namespace Blueprint.Core.Api.Middleware
{
    public class ValidationFailedResult : OkResult<ValidationErrorResponse>
    {
        public ValidationFailedResult(string formLevelErrorMessage)
            : this(new ValidationErrorResponse(new Dictionary<string, IEnumerable<string>>
            {
                [ValidationFailures.FormLevelPropertyName] = new [] { formLevelErrorMessage }
            }))
        {
        }

        public ValidationFailedResult(Dictionary<string, IEnumerable<string>> errors)
            : this(new ValidationErrorResponse(errors))
        {
        }

        public ValidationFailedResult(ValidationFailures errors)
            : this(new ValidationErrorResponse(errors))
        {
        }

        public ValidationFailedResult(ValidationErrorResponse content) : base((HttpStatusCode) 422, content)
        {
        }
    }
}
