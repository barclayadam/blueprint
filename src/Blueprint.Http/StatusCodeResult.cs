using System.Net;

namespace Blueprint.Http;

/// <summary>
/// <para>
/// A simple <see cref="HttpResult" /> that can be used when no content needs writing, only a status code and (optional)
/// headers.
/// </para>
/// <para>
/// It is recommended to declare return types as a specific subclass of this (i.e. <see cref="StatusCodeResult.Created" />)
/// to provide additional metadata with regards to expected responses to enable a more comprehensive and accurate OpenApi
/// document to be created.
/// </para>
/// </summary>
public partial class StatusCodeResult : HttpResult
{
    /// <summary>
    /// Initialises a new instance of the <see cref="StatusCodeResult" /> class.
    /// </summary>
    /// <param name="statusCode">The status code to write.</param>
    public StatusCodeResult(HttpStatusCode statusCode)
        : base(statusCode)
    {
    }
}