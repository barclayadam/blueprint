using System.Net;
using Blueprint.Api.Authorisation;

namespace Blueprint.Api.Errors
{
    public class ForbiddenException : ApiException
    {
        public ForbiddenException(ExecutionAllowed failure)
            : base(failure.Message, "unauthorized", failure.Reason, (int)HttpStatusCode.Forbidden)
        {
            FailureReason = failure;
        }

        /// <summary>
        /// Gets the reason for this <see cref="ForbiddenException" />.
        /// </summary>
        public ExecutionAllowed FailureReason { get; }
    }
}
