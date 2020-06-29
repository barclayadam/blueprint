using System;
using System.Threading.Tasks;
using Blueprint.Middleware;

namespace Blueprint.Http
{
    /// <summary>
    /// An <see cref="IContextMetadataProvider" /> that populates metadata from the HTTP request
    /// such as IP Address and UserAgent.
    /// </summary>
    public class HttpContextMetadataProvider : IContextMetadataProvider
    {
        /// <inheritdoc />
        public Task PopulateMetadataAsync(ApiOperationContext context, Action<string, object> add)
        {
            var request = context.GetHttpContext().Request;

            add("IpAddress", request.GetClientIpAddress());

            if (request.Headers.ContainsKey("User-Agent"))
            {
                add(
                    "UserAgent",
                    string.Join(" ", request.Headers["User-Agent"].ToString()));
            }

            return Task.CompletedTask;
        }
    }
}
