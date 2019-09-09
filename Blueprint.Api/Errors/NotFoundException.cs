using System.Net;

namespace Blueprint.Api.Errors
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message)
            : base(message, "not_found", HttpStatusCode.NotFound)
        {
        }
    }
}