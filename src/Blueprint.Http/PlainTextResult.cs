using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

namespace Blueprint.Http;

/// <summary>
/// A <see cref="HttpResult" /> that sets the content type to "text/plain" and writes the given
/// string content to the output stream.
/// </summary>
public class PlainTextResult : HttpResult
{
    private readonly string _content;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlainTextResult" /> class with the given string
    /// content to write to the response stream, using a HTTP status code of 200 (OK).
    /// </summary>
    /// <param name="content">The content to write.</param>
    public PlainTextResult(string content)
        : base(HttpStatusCode.OK)
    {
        this._content = content;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlainTextResult" /> class with the given string
    /// content to write to the response stream.
    /// </summary>
    /// <param name="content">The content to write.</param>
    /// <param name="statusCode">The status code of this response.</param>
    public PlainTextResult(string content, HttpStatusCode statusCode)
        : base(statusCode)
    {
        this._content = content;
    }

    /// <summary>
    /// The content that is to be written.
    /// </summary>
    public string Content => this._content;

    /// <summary>
    /// Gets or sets the content type header, which defaults to <c>text/plain</c>.
    /// </summary>
    public string ContentType { get; set; } = "text/plain";

    /// <inheritdoc />
    public override async Task ExecuteAsync(ApiOperationContext context)
    {
        await base.ExecuteAsync(context);

        var httpContext = context.GetHttpContext();
        var response = httpContext.Response;

        response.ContentType = this.ContentType;

        using var httpResponseStreamWriter = new HttpResponseStreamWriter(response.Body, Encoding.UTF8);

        await httpResponseStreamWriter.WriteAsync(this._content);
        await httpResponseStreamWriter.FlushAsync();
    }
}