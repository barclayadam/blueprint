using System;
using System.ComponentModel;

namespace Blueprint
{
    /// <summary>
    /// Represents a resource key, a string that can uniquely identify a resource (e.g. an entity such as
    /// a user or an address), in the format of <em>[Type]</em>/<em>[Id]</em> where [Id] is a <see cref="Guid"/>.
    /// </summary>
    /// <typeparam name="T">The type of resource (typically an ApiResource) being represented.</typeparam>
    public class ResourceKey<T>
    {
        private readonly T _id;
        private readonly string _resourceType;

        private ResourceKey(string resourceType, T id)
        {
            this._id = id;
            this._resourceType = resourceType;
        }

        /// <summary>
        /// Gets the ID of the resource this resource key represents.
        /// </summary>
        public T Id => this._id;

        /// <summary>
        /// Gets the type this resource key represents.
        /// </summary>
        public string ResourceType => this._resourceType;

        /// <summary>
        /// Converts the string representation of a ResourceKey to the equivalent ResourceKey structure.
        /// </summary>
        /// <param name="resourceKey">
        /// The resource key to convert.
        /// </param>
        /// <returns>
        /// The parsed resource key.
        /// </returns>
        public static ResourceKey<T> Parse(string resourceKey)
        {
            ResourceKey<T> parsedResourceKey;

            if (TryParse(resourceKey, out parsedResourceKey))
            {
                return parsedResourceKey;
            }

            throw new FormatException(
                $"Resource key '{resourceKey}' is in an invalid format. A resource key must be in the format '[Type]/[Id]' where Id is a {typeof(T).Name}.");
        }

        /// <summary>
        /// Extracts the id out of the given string representation of a ResourceKey.
        /// </summary>
        /// <param name="resourceKey">
        /// The resource key to convert.
        /// </param>
        /// <returns>
        /// The parsed resource key's id.
        /// </returns>
        public static T ParseId(string resourceKey)
        {
            return Parse(resourceKey).Id;
        }

        /// <summary>
        /// Converts the string representation of a GUID to the equivalent Guid structure.
        /// </summary>
        /// <param name="resourceKey">
        /// The resource key to convert.
        /// </param>
        /// <param name="parsedResourceKey">
        /// The structure that will contain the parsed value.
        /// </param>
        /// <returns>
        /// True if the resource key can be parsed.
        /// </returns>
        public static bool TryParse(string resourceKey, out ResourceKey<T> parsedResourceKey)
        {
            if (string.IsNullOrEmpty(resourceKey))
            {
                parsedResourceKey = null;
                return true;
            }

            var resource = resourceKey.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (resource.Length != 2)
            {
                parsedResourceKey = null;
                return false;
            }

            var type = resource[0];

            if (string.IsNullOrEmpty(type))
            {
                parsedResourceKey = null;
                return false;
            }

            try
            {
                parsedResourceKey = new ResourceKey<T>(type, (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(resource[1]));
                return true;
            }
            catch (FormatException)
            {
                parsedResourceKey = null;
                return false;
            }
            catch (Exception)
            {
                // This is thrown by System.ComponentModel.BaseNumberConverter.ConvertFrom
                parsedResourceKey = null;
                return false;
            }
        }

        /// <summary>
        /// Returns the string representation of the resource key in for format of [Type]/[Id].
        /// </summary>
        /// <returns>
        /// The resource key string.
        /// </returns>
        public override string ToString()
        {
            return this._resourceType + "/" + this._id;
        }
    }
}
