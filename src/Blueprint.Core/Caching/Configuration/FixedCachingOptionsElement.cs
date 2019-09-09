using System;
using System.Configuration;

namespace Blueprint.Core.Caching.Configuration
{
    /// <summary>
    /// A fixed time cache options element, providing ethe ability to specify an absolute
    /// expiration time which is a timespan from when the cache element is <strong>First added</strong>
    /// to the cache.
    /// </summary>
    public class FixedCachingOptionsElement : CachingOptionsElement
    {
        /// <summary>
        /// Gets the time span that determines the length of time that an item will be stored
        /// in a cache from the moment it is first inserted.
        /// </summary>
        [ConfigurationProperty("timeSpan", IsRequired = true)]
        public TimeSpan TimeSpan { get { return (TimeSpan)this["timeSpan"]; } }

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