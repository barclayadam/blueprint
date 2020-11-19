using System.Net;
using Blueprint.Authorisation;

namespace Blueprint.Errors
{
    public class ForbiddenException : ApiException
    {
        public ForbiddenException(ExecutionAllowed failure)
            : base(failure.Message, "unauthorized", failure.Reason, (int)HttpStatusCode.Forbidden)
        {
            this.FailureReason = failure;
        }

        /// <summary>
        /// Gets the reason for this <see cref="ForbiddenException" />.
        /// </summary>
        public ExecutionAllowed FailureReason { get; }
    }
}
