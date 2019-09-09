using System.Net;

using Blueprint.Core.Api;

namespace Blueprint.Core.Errors
{
    public class BadRequestException : ApiException
    {
        public BadRequestException(string message)
            : base(message, "bad_request", HttpStatusCode.BadRequest)
        {
        }
    }
}