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
    private readonly ApiDataModel _apiDataModel;
    private readonly string _baseUri;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiLinkGenerator"/> class.
    /// </summary>
    /// <param name="apiDataModel">The API data model this generator is for.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public ApiLinkGenerator(ApiDataModel apiDataModel, IHttpContextAccessor httpContextAccessor)
    {
        Guard.NotNull(nameof(apiDataModel), apiDataModel);
        Guard.NotNull(nameof(httpContextAccessor), httpContextAccessor);

        this._apiDataModel = apiDataModel;
        this._baseUri = httpContextAccessor.HttpContext.GetBlueprintBaseUri();
    }

    /// <inheritdoc />
    public Link CreateSelfLink<T>(int id, object queryString = null) where T : ApiResource
    {
        return this.CreateSelfLink<T>(new { id }, queryString);
    }

    /// <inheritdoc />
    public Link CreateSelfLink<T>(long id, object queryString = null) where T : ApiResource
    {
        return this.CreateSelfLink<T>(new { id }, queryString);
    }

    /// <inheritdoc />
    public Link CreateSelfLink<T>(string id, object queryString = null) where T : ApiResource
    {
        return this.CreateSelfLink<T>(new { id }, queryString);
    }

    /// <inheritdoc />
    public Link CreateSelfLink<T>(Guid id, object queryString = null) where T : ApiResource
    {
        return this.CreateSelfLink<T>(new { id }, queryString);
    }

    /// <inheritdoc />
    public Link CreateSelfLink<T>(object idDefinition, object queryString = null) where T : ApiResource
    {
        Guard.NotNull(nameof(idDefinition), idDefinition);

        var selfLink = this._apiDataModel.GetLinkFor(typeof(T), "self");

        if (selfLink == null)
        {
            throw new InvalidOperationException(
                $"Cannot generate a self link for the resource type {typeof(T).Name} as one has not been registered. Make sure an operation link has " +
                "been registered with the ApiDataModel of this generator with a rel of 'self', which can be achieved by using the [SelfLink] attribute on an IApiOperation.");
        }

        // baseUri always has / at end, relative never has at start
        var relativeUrl = selfLink.CreateRelativeUrl(idDefinition);

        var fullUri = new StringBuilder(this._baseUri.Length + relativeUrl.Length);

        fullUri.Append(this._baseUri);
        fullUri.Append(relativeUrl);

        if (queryString != null)
        {
            AppendAsQueryString(fullUri, queryString.GetType().GetProperties(), queryString, p => true);
        }

        return new Link
        {
            Href = fullUri.ToString(),
            Type = ApiResource.GetTypeName(typeof(T)),
        };
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

    /// <inheritdoc />
    public string CreateUrl(object operation)
    {
        var operationType = operation.GetType();
        var link = this._apiDataModel.GetLinksForOperation(operationType).FirstOrDefault();

        if (link == null)
        {
            throw new InvalidOperationException($"No links exist for the operation {operationType.FullName}.");
        }

        var relativeUrl = link.CreateRelativeUrl(operation, true);

        // We cannot create a full URL if the relative link is null
        if (relativeUrl == null)
        {
            return null;
        }

        return this._baseUri + relativeUrl;
    }

    private static void AppendAsQueryString(
        StringBuilder routeUrl,
        PropertyInfo[] properties,
        object values,
        Func<PropertyInfo, bool> shouldInclude)
    {
        var addedQs = false;

        // Now, for every property that has a value but has NOT been placed in to the route will be added as a query string
        foreach (var property in properties)
        {
            // This property has already been handled by the route generation generation above
            if (!shouldInclude(property))
            {
                continue;
            }

            var value = property.GetValue(values, null);

            // Ignore default values, they are unnecessary to pass back through the URL
            if (value == null || Equals(GetDefaultValue(property.PropertyType), value))
            {
                continue;
            }

            if (!addedQs)
            {
                routeUrl.Append('?');
                addedQs = true;
            }
            else
            {
                routeUrl.Append('&');
            }

            routeUrl.Append(property.Name);
            routeUrl.Append('=');
            routeUrl.Append(Uri.EscapeDataString(value.ToString()));
        }
    }

    private static object GetDefaultValue(Type t)
    {
        return t.IsValueType ? Activator.CreateInstance(t) : null;
    }
}