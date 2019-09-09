namespace Blueprint.Core.Caching
{
    /// <summary>
    /// Implements the port between the <see cref="ICache"/> implementation and a specific caching
    /// technology (e.g. ASP.Net Cache, Redis).
    /// </summary>
    /// <remarks>
    /// A cache provider can assume that the keys, values and options have all been checked, managed
    /// and manipulated before being called, and can therefore provide a relatively light shim
    /// over the underlying technology for each method without having to worry around whether to
    /// actually cache a value of worrying about key clashes.
    /// </remarks>
    public interface ICacheProvider
    {
        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <remarks>
        /// If the value supplies is <c>null</c> this method becomes a no-op.
        /// </remarks>
        /// <param name="key">
        /// The key used for identifying the value.
        /// </param>
        /// <param name="value">
        /// The value to be stored.
        /// </param>
        /// <param name="options">
        /// The options used to store the value.
        /// </param>
        void Add(string key, object value, CacheOptions options);

        /// <summary>
        /// Returns a value indicating whether or not the cache contains a value against
        /// the given key.
        /// </summary>
        /// <param name="key">
        /// The unique identifier of the item.
        /// </param>
        /// <returns>
        /// Whether a value for the key exists within the cache.
        /// </returns>
        bool ContainsKey(string key);

        /// <summary>
        /// Gets a value from this cache that has been previously stored using
        /// the specified key.
        /// </summary>
        /// <param name="key">
        /// The key that was used to store a value.
        /// </param>
        /// <returns>
        /// The value that had been previously stored for the key.
        /// </returns>
        object GetValue(string key);

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to remove the value for.
        /// </param>
        void Remove(string key);
    }
}