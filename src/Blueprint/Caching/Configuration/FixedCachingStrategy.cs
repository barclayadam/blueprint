using System;

namespace Blueprint.Caching.Configuration
{
    /// <summary>
    /// A fixed time cache options element, providing ethe ability to specify an absolute
    /// expiration time which is a timespan from when the cache element is <strong>First added</strong>
    /// to the cache.
    /// </summary>
    public class FixedCachingStrategy : CachingStrategy
    {
        /// <summary>
        /// Gets or sets the time span that determines the length of time that an item will be stored
        /// in a cache from the moment it is first inserted.
        /// </summary>
        public TimeSpan TimeSpan { get; set; }

        /// <summary>
        /// Gets the options for the specified value, which will be an absolute cache
        /// options instance.
        /// </summary>
        /// <returns>
        /// The cache options to use when storing the specified value.
        /// </returns>
        /// <seealso cref="CacheOptions.Absolute"/>
        public override CacheOptions GetOptions()
        {
            return CacheOptions.Absolute(ItemPriority, TimeSpan);
        }
    }
}
