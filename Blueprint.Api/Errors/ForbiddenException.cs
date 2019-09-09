using System.Net;

namespace Blueprint.Api.Errors
{
    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message)
            : base(message, "unauthorized", HttpStatusCode.Forbidden)
        {
        }
    }
}
