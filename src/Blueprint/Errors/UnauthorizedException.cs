using System.Net;

namespace Blueprint.Errors
{
    /// <summary>
    /// An <see cref="ApiException" /> thrown when the user is unauthorised to execute the request, which means
    /// that some authentication was required but the user is anonymous.
    /// </summary>
    public class UnauthorizedException : ApiException
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="UnauthorizedException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public UnauthorizedException(string message)
            : base(message, "security_failure", null, (int)HttpStatusCode.Unauthorized)
        {
        }
    }
}
