using System.Collections.Generic;
using Blueprint.Http.Formatters;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Http;

/// <summary>
/// A set of options that are used throughout the Blueprint HTTP support module.
/// </summary>
public class BlueprintHttpOptions
{
    /// <summary>
    /// The list of available <see cref="IOperationResultOutputFormatter" />s.
    /// </summary>
    public List<IOperationResultOutputFormatter> OutputFormatters { get; } = new List<IOperationResultOutputFormatter>();

    /// <summary>
    /// Whether to expose exception details when formatting unhandled exceptions in the pipeline. If this
    /// is <c>true</c> we will populate a formatted <see cref="ProblemDetails" /> instance with the exception message
    /// and stack trace, otherwise a generic message will be used and no stack trace will be present.
    /// </summary>
    public bool ExposeExceptionDetailsInErrorResponses { get; set; }

    /// <summary>
    /// The list of available <see cref="IBodyParser" />s.
    /// </summary>
    public List<IBodyParser> BodyParsers { get; } = new List<IBodyParser>();

    /// <summary>
    /// Gets or sets the flag which causes content negotiation to ignore Accept header
    /// when it contains the media type */*. <see langword="false"/> by default.
    /// </summary>
    public bool RespectBrowserAcceptHeader { get; set; }

    /// <summary>
    /// Gets or sets the flag which decides whether an HTTP 406 Not Acceptable response
    /// will be returned if no formatter has been selected to format the response.
    /// <see langword="false"/> by default.
    /// </summary>
    public bool ReturnHttpNotAcceptable { get; set; }

    /// <summary>
    /// An optional domain that should be used instead of the request URL when generating absolute
    /// link URLs.
    /// </summary>
    /// <remarks>
    /// By default this is <c>null</c> which means when generating URLs Blueprint will look at the
    /// incoming HTTP request and use the <see cref="HttpRequest.Host" /> property.
    /// </remarks>
    [CanBeNull]
    public string PublicHost { get; set; }
}