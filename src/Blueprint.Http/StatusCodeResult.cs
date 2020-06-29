using System.Net;

namespace Blueprint.Http
{
    /// <summary>
    /// A simple <see cref="HttpResult" /> that can be used when no content needs writing, only a status code and (optional)
    /// headers.
    /// </summary>
    public class StatusCodeResult : HttpResult
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="StatusCodeResult" /> class.
        /// </summary>
        /// <param name="statusCode">The status code to write.</param>
        public StatusCodeResult(HttpStatusCode statusCode) : base(statusCode)
        {
        }
    }
}
