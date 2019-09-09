using System;
using System.Collections.Generic;
using Blueprint.Core.ThirdParty;
using Blueprint.Core.Utilities;
using Newtonsoft.Json;

namespace Blueprint.Api
{
    /// <summary>
    /// An ApiResource that can be returned from an operation that has a standard format
    /// to be consumed by clients, including resource type ($object), links ($links) and whether this
    /// represents a 'partial' response ($partial).
    /// </summary>
    public class ApiResource : ILinkableResource
    {
        private static readonly Dictionary<Type, string> TypeNameCache = new Dictionary<Type, string>();

        private readonly Dictionary<string, Link> links = new Dictionary<string, Link>(5);

        /// <summary>
        /// Initializes a new instance of ApiResource, setting the <see cref="Object"/> property
        /// to a conventional name of the implementing class being passed to <see cref="GetTypeName"/>.
        /// </summary>
        public ApiResource()
        {
            Object = GetTypeName(GetType());
        }

        /// <summary>
        /// Gets the object type of this resource, used to indicate to clients what they
        /// are dealing with.
        /// </summary>
        [JsonProperty(PropertyName = "$object")]
        public string Object { get; protected set; }

        /// <summary>
        /// Gets the links that have currently been defined for this resource.
        /// </summary>
        [DoNotCompare]
        [JsonProperty(PropertyName = "$links")]
        public IDictionary<string, Link> Links => links;

        [JsonIgnore]
        public virtual string ResourceKey => null;

        /// <summary>
        /// Gets or sets a value indicating whether or not this resource is 'partial', an indication that not all properties have been
        /// populated for performance reasons and a client should reload from the 'self' link to retrieve all values.
        /// </summary>
        [JsonProperty(PropertyName = "$partial")]
        public bool IsPartial { get; set; }

        /// <summary>
        /// Gets the type name (exposed as 'object' from the API) of the given resource type, applying
        /// default conventions of removing 'Resource' and 'DTO' string from the type name.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        public static string GetTypeName(Type resourceType)
        {
            return TypeNameCache.GetOrAdd(
                resourceType,
                t => t.Name.Replace("Resource", string.Empty).Camelize());
        }

        /// <summary>
        /// Adds a new link with the specific rel (relation) and <see cref="Link"/>
        /// definition, throwing an exception if a link with the specific relation already
        /// exists.
        /// </summary>
        /// <param name="rel">The relation the link has to this resource.</param>
        /// <param name="link">The link to be added.</param>
        public void AddLink(string rel, Link link)
        {
            // Allow exception to bubble if link already exists. Allow normal success path to be quicker instead
            // of checking for duplicates (could do so in tests?)
            links.Add(rel, link);
        }
    }
}
