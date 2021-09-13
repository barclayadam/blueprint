using Newtonsoft.Json;

namespace Blueprint
{
    /// <summary>
    /// A link from one resource to another, that indicates what operations may be available
    /// for a given <see cref="ILinkableResource" /> being returned from the API.
    /// </summary>
    /// <seealso cref="RootLinkAttribute" />
    /// <seealso cref="LinkAttribute" />
    /// <seealso cref="SelfLinkAttribute" />
    public record Link
    {
        /// <summary>
        /// The URL of this link.
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// The type of resource this link will return.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }
}
