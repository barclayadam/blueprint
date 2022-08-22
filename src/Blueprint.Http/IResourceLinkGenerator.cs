using System.Threading.Tasks;

namespace Blueprint.Http;

/// <summary>
/// A resource link generator provides the means to generate the links that are presented with
/// a resource, which would contain the actions that could be performed to change state, in addition
/// to links to related entities (e.g. the members of an account).
/// </summary>
public interface IResourceLinkGenerator
{
    ValueTask AddLinksAsync(IApiLinkGenerator apiLinkGenerator, ApiOperationContext context, ILinkableResource linkableResource);
}