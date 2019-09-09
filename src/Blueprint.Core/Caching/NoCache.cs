namespace Blueprint.Core.Caching
{
    /// <summary>
    /// Implements <see cref="ICache" /> to be used in places that required no caching, for example in
    /// tests.
    /// </summary>
    public class NoCache : ICache
    {
        /// <summary>
        /// Does nothing, will not add the item to cache.
        /// </summary>
        /// <param name="category">A category the value belongs to, used to specify 'profiles' of caching via.
        /// configuration such as 'Content', or 'Reports'.</param>
        /// <param name="key">The key used for identifying the value.</param>
        /// <param name="value">The value to be stored.</param>
        /// <typeparam name="T">The type of the value being inserted / retrieved, inferred by the return type
        /// of the constructor.</typeparam>
        public void Add<T>(string category, object key, T value)
        {
        }

        /// <summary>
        /// Always returns false, as no items are ever added to this cache.
        /// </summary>
        /// <param name="key">The unique identifier of the item.</param>
        /// <typeparam name="T">The type of the value being checked for existence</typeparam>
        /// <returns><c>false</c></returns>
        public bool ContainsKey<T>(object key)
        {
            return false;
        }

        /// <summary>
        /// Returns <c>default(T)</c>, as no items are ever added to this cache.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="key">The unique key to get.</param>
        /// <returns>The value of <c>default(T)</c>.</returns>
        public T GetValue<T>(object key)
        {
            return default(T);
        }

        /// <summary>
        /// Does nothing, as nothing is ever added to the cache.
        /// </summary>
        /// <typeparam name="T">The type of the value to remove.</typeparam>
        /// <param name="key">The unique key to remove.</param>
        public void Remove<T>(object key)
        {
        }
    }
}