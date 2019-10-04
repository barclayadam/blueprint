using System.Net;
using System.Threading.Tasks;

namespace Blueprint.Api.Http
{
    /// <summary>
    /// A simple <see cref="HttpResult" /> that can be used when no content needs writing, only a status code and (optional)
    /// headers.
    /// </summary>
    public class StatusCodeResult : HttpResult
    {
        public StatusCodeResult(HttpStatusCode statusCode) : base(statusCode)
        {
        }
    }
}
