using System.Net;
using Blueprint.Api.Authorisation;

namespace Blueprint.Api.Errors
{
    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message)
            : this(ExecutionAllowed.No(message, message, ExecutionAllowedFailureType.Authorisation))
        {
        }

        public ForbiddenException(ExecutionAllowed failure)
            : base(failure.Reason, "unauthorized", HttpStatusCode.Forbidden)
        {
            FailureReason = failure;
        }

        /// <summary>
        /// Gets the reason for this <see cref="ForbiddenException" />.
        /// </summary>
        public ExecutionAllowed FailureReason { get; }

        /// <summary>
        /// Gets the API error message for this exception, which will either come from the
        /// <see cref="ExecutionAllowed.Message" /> property of this exception's <see cref="FailureReason" />.
        /// </summary>
        public override string ErrorMessage => FailureReason.Message;
    }
}
