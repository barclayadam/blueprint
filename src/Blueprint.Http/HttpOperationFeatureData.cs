using System;
using System.Collections.Generic;

namespace Blueprint.Http;

/// <summary>
/// Feature data for <see cref="ApiOperationDescriptor" />s relating to HTTP.
/// </summary>
public class HttpOperationFeatureData
{
    private readonly List<ApiOperationLink> _links = new();

    /// <summary>
    /// Initialises a new instance of the <see cref="HttpOperationFeatureData" /> class.
    /// </summary>
    /// <param name="httpHost">The creator of this instance.</param>
    /// <param name="httpMethod">The HTTP method of this operation.</param>
    public HttpOperationFeatureData(HttpHost httpHost, string httpMethod)
    {
        this.HttpHost = httpHost;
        this.HttpMethod = httpMethod;
    }

    /// <summary>
    /// The <see cref="HttpHost" /> that created this feature data.
    /// </summary>
    public HttpHost HttpHost { get; }

    /// <summary>
    /// Gets the (uppercase) HTTP method of this operation.
    /// </summary>
    public string HttpMethod { get; }

    /// <summary>
    /// Gets all registered <see cref="ApiOperationLink" />s for this descriptor.
    /// </summary>
    public IReadOnlyList<ApiOperationLink> Links => this._links;

    /// <summary>
    /// Adds a link for this descriptor.
    /// </summary>
    /// <param name="apiOperationLink">The link to add.</param>
    /// <exception cref="NotImplementedException">If the link has not been created for this descriptor.</exception>
    public void AddLink(ApiOperationLink apiOperationLink)
    {
        this._links.Add(apiOperationLink);
    }
}
