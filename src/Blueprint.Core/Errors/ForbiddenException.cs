using System.Net;

using Blueprint.Core.Api;

namespace Blueprint.Core.Errors
{
    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message)
            : base(message, "unauthorized", HttpStatusCode.Forbidden)
        {
        }
    }
}