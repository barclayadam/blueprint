using System.Collections.Generic;

namespace Blueprint;

/// <summary>
/// Represents a linkable resource, a resource that can have links attached that direct users
/// of the API to other actions or resources related to this one.
/// </summary>
public interface ILinkableResource
{
    /// <summary>
    /// Gets the links that have currently been defined for this resource.
    /// </summary>
    IDictionary<string, Link> Links { get; }

    /// <summary>
    /// Adds a new link with the specific rel (relation) and <see cref="Link"/>
    /// definition, throwing an exception if a link with the specific relation already
    /// exists.
    /// </summary>
    /// <param name="rel">The relation the link has to this resource.</param>
    /// <param name="link">The link to be added.</param>
    void AddLink(string rel, Link link);
}