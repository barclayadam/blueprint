namespace Blueprint.Http;

/// <summary>
/// Feature data for <see cref="ApiOperationDescriptor" />s relating to HTTP.
/// </summary>
public class HttpOperationFeatureData
{
    /// <summary>
    /// Initialises a new instance of the <see cref="HttpOperationFeatureData" /> class.
    /// </summary>
    /// <param name="httpMethod">The HTTP method of this operation.</param>
    public HttpOperationFeatureData(string httpMethod)
    {
        this.HttpMethod = httpMethod;
    }

    /// <summary>
    /// Gets the (uppercase) HTTP method of this operation.
    /// </summary>
    public string HttpMethod { get; }
}