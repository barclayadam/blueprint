using Blueprint.Configuration;

namespace Blueprint.Http;

public interface IApiLinkGenerator
{
    /// <summary>
    /// Creates a fully qualified URL (using <see cref="BlueprintApiOptions.BaseApiUrl" />) for the specified link
    /// and "result" object that is used to fill the placeholders of the link.
    /// </summary>
    /// <param name="link">The link to generate URL for.</param>
    /// <param name="result">The "result" object used to populate placeholder values of the specified link route.</param>
    /// <returns>A fully-qualified URL.</returns>
    string CreateUrl(ApiOperationLink link, object result = null);
}
