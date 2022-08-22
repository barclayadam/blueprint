using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Blueprint.Http;

/// <summary>
/// Container for HTTP-related context information such as the <see cref="RouteData" /> and
/// <see cref="HttpContext" />.
/// </summary>
public class HttpFeatureContext
{
    /// <summary>
    /// Gets the HTTP context of this operation context.
    /// </summary>
    public HttpContext HttpContext { get; internal set; }

    /// <summary>
    /// The route data of this operation context.
    /// </summary>
    public RouteData RouteData { get; internal set; }
}