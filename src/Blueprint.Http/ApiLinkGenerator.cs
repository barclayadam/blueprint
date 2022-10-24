using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Http;

/// <summary>
/// The API link generator is responsible for creating the URLs that would be used, for example, to generate
/// the $links properties of returned resources from registered <see cref="ApiOperationLink"/>s.
/// </summary>
public class ApiLinkGenerator : IApiLinkGenerator
{
    private readonly string _baseUri;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiLinkGenerator"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public ApiLinkGenerator(IHttpContextAccessor httpContextAccessor)
    {
        Guard.NotNull(nameof(httpContextAccessor), httpContextAccessor);

        this._baseUri = httpContextAccessor.HttpContext.GetBlueprintBaseUri();
    }

    /// <inheritdoc />
    public string CreateUrl(ApiOperationLink link, object result = null)
    {
        // We can short-circuit in the (relatively uncommon case) of no placeholders
        if (!link.HasPlaceholders())
        {
            return this._baseUri + link.UrlFormat;
        }

        var relativeUrl = link.CreateRelativeUrl(result);

        // We cannot create a full URL if the relative link is null
        if (relativeUrl == null)
        {
            return null;
        }

        // baseUri always has / at end, relative never has at start
        return this._baseUri + relativeUrl;
    }
}
