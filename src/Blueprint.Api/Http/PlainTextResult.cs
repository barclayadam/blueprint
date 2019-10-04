using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

namespace Blueprint.Api.Http
{
    /// <summary>
    /// A <see cref="HttpResult" /> that sets the content type to "text/plain" and writes the given
    /// string content to the output stream.
    /// </summary>
    public class PlainTextResult : HttpResult
    {
        private readonly string content;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlainTextResult" /> class with the given string
        /// content to write to the response stream.
        /// </summary>
        /// <param name="content">The content to write.</param>
        public PlainTextResult(string content) : base(HttpStatusCode.OK)
        {
            this.content = content;
        }

        /// <inheritdoc />
        public override async Task ExecuteAsync(ApiOperationContext context)
        {
            await base.ExecuteAsync(context);

            context.Response.ContentType = "text/plain";

            using (var httpResponseStreamWriter = new HttpResponseStreamWriter(context.Response.Body, Encoding.UTF8))
            {
                httpResponseStreamWriter.Write(content);

                await httpResponseStreamWriter.FlushAsync();
            }
        }
    }
}
