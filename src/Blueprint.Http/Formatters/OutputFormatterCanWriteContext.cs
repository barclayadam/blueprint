using Microsoft.AspNetCore.Http;

namespace Blueprint.Http.Formatters;

/// <summary>
/// A context used when determining what <see cref="IOperationResultOutputFormatter" /> to use and
/// it's subsequent use.
/// </summary>
public class OutputFormatterCanWriteContext
{
    /// <summary>
    /// Initialises a new instance of the <see cref="OutputFormatterCanWriteContext" /> class.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="result">The pipeline result.</param>
    public OutputFormatterCanWriteContext(HttpContext httpContext, object result)
    {
        this.Response = httpContext.Response;
        this.Request = httpContext.Request;
        this.Result = result;
        this.ContentType = default;
    }

    /// <summary>
    /// The HTTP response.
    /// </summary>
    public HttpResponse Response { get; }

    /// <summary>
    /// The HTTP request.
    /// </summary>
    public HttpRequest Request { get; }

    /// <summary>
    /// The result of the operation.
    /// </summary>
    public object Result { get; }

    /// <summary>
    /// The content type to check for / use.
    /// </summary>
    public MediaType? ContentType { get; set; }
}