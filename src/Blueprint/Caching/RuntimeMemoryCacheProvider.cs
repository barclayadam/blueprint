using System;
using System.Runtime.Caching;

namespace Blueprint.Caching
{
    public class RuntimeMemoryCacheProvider : ICacheProvider
    {
        private readonly MemoryCache _memoryCache;

        /// <summary>
        /// Initializes a new instance of the memoryCacheProvider class using the
        /// provided  System.Runtime.Caching cache.
        /// </summary>
        /// <param name="memoryCache">
        /// The instance of <see cref="MemoryCache"/> used as the actual cache storage.
        /// </param>
        public RuntimeMemoryCacheProvider(MemoryCache memoryCache)
        {
            Guard.NotNull(nameof(memoryCache), memoryCache);

            this._memoryCache = memoryCache;
        }

        /// <summary>
        /// Performs the actual insert of a key / value pair and the options that apply to
        /// this cache item.
        /// </summary>
        /// <param name="key">
        /// The key of the item being inserted.
        /// </param>
        /// <param name="value">
        /// The non-null value to be stored.
        /// </param>
        /// <param name="options">
        /// The options of this cache item.
        /// </param>
        public void Add(string key, object value, CacheOptions options)
        {
            this._memoryCache.Add(key, value, new CacheItemPolicy
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                SlidingExpiration = options.SlidingExpiration,
                Priority = ConvertPriority(options.Priority),
            });
        }

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
        public bool ContainsKey(string key)
        {
            return this._memoryCache.Contains(key);
        }

        /// <summary>
        /// Gets the value stored in this cache with the given key.
        /// </summary>
        /// <param name="key">
        /// The unique key that represents the cache item to retrieve.
        /// </param>
        /// <returns>
        /// The value that has been stored in this cache for the given key, or <c>null</c>.
        /// </returns>
        public object GetValue(string key)
        {
            return this._memoryCache[key];
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to remove the value for.
        /// </param>
        public void Remove(string key)
        {
            this._memoryCache.Remove(key);
        }

        private static System.Runtime.Caching.CacheItemPriority ConvertPriority(CacheItemPriority priority)
        {
            switch (priority)
            {
                case CacheItemPriority.Low:
                case CacheItemPriority.Medium:
                    return System.Runtime.Caching.CacheItemPriority.Default;

                case CacheItemPriority.High:
                    return System.Runtime.Caching.CacheItemPriority.NotRemovable;
            }

            throw new InvalidOperationException(
                $"Could not convert the cache item priority of '{priority}' to its System.Runtime.Caching.CacheItemPriority equivalent.");
        }
    }
}
