using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching;

namespace Blueprint.Caching
{
    /// <summary>
    /// The options used when inserting a value into the cache.
    /// </summary>
    public sealed class CacheOptions
    {
        /// <summary>
        /// CacheOption which is used to indicate a value should not be stored in a cache.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "CacheOptions is an immutable type")]
        public static readonly CacheOptions NotCached = new CacheOptions();

        private CacheOptions()
        {
            Priority = CacheItemPriority.Medium;

            AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;
            SlidingExpiration = ObjectCache.NoSlidingExpiration;
        }

        /// <summary>
        /// Gets the absolute expiration time for a cache item, the latest time in which
        /// the item can still be retrieved from the cache before it is automatically discarded.
        /// </summary>
        public DateTimeOffset AbsoluteExpiration { get; private set; }

        /// <summary>
        /// Gets the priority at which a item should be entered into a cache, providing
        /// a hint to the cache as to what items can be discarded first if required.
        /// </summary>
        public CacheItemPriority Priority { get; private set; }

        /// <summary>
        /// Gets the sliding expiration for a cache item, a period of time which is used
        /// to remove an item after its last access.
        /// </summary>
        public TimeSpan SlidingExpiration { get; private set; }

        /// <summary>
        /// Initializes a new instance of the CacheOptions class which has an absolute expiration
        /// date.
        /// </summary>
        /// <param name="priority">
        /// The priority of this cached item.
        /// </param>
        /// <param name="absoluteExpiration">
        /// The time, which must be in the future, at which this item
        /// will be evicted.
        /// </param>
        /// <returns>
        /// A <see cref="CacheOptions"/> instance representing the absolute expiration.
        /// </returns>
        public static CacheOptions Absolute(CacheItemPriority priority, TimeSpan absoluteExpiration)
        {
            return new CacheOptions
            {
                Priority = priority,
                AbsoluteExpiration = SystemTime.UtcNow.Add(absoluteExpiration),
            };
        }

        /// <summary>
        /// Initializes a new instance of the CacheOptions class which has a sliding expiration.
        /// </summary>
        /// <param name="priority">
        /// The priority of this cached item.
        /// </param>
        /// <param name="slidingExpiration">
        /// The amount of time after the last access to this item after which this
        /// item becomes invalid.
        /// </param>
        /// <returns>
        /// A <see cref="CacheOptions"/> instance representing the sliding expiration.
        /// </returns>
        public static CacheOptions Sliding(CacheItemPriority priority, TimeSpan slidingExpiration)
        {
            return new CacheOptions
            {
                Priority = priority,
                SlidingExpiration = slidingExpiration,
            };
        }

        /// <summary>
        /// Returns a string representation of this <see cref="CacheOptions"/> instance, indicating whether
        /// it is sliding or absolute and indicating timespan and priority options that have been set.
        /// </summary>
        /// <returns>The string representation of this CacheOptions instance.</returns>
        public override string ToString()
        {
            if (SlidingExpiration != ObjectCache.NoSlidingExpiration)
            {
                return $"[Sliding] Expiration = {SlidingExpiration}; Priority = {Priority}";
            }

            return $"[Absolute] Expiration = {AbsoluteExpiration}; Priority = {Priority}";
        }
    }
}
