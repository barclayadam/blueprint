using System.Net;

using Blueprint.Core.Api;

namespace Blueprint.Core.Errors
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message)
            : base(message, "not_found", HttpStatusCode.NotFound)
        {
        }
    }
}