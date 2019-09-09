using System.Net;

namespace Blueprint.Api.Errors
{
    public class BadRequestException : ApiException
    {
        public BadRequestException(string message)
            : base(message, "bad_request", HttpStatusCode.BadRequest)
        {
        }
    }
}
