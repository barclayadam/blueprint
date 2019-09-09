using System;

namespace Blueprint.Core.Caching.Configuration
{
    /// <summary>
    /// A caching options element which defines the options for a type (or collection
    /// of using regular expressions).
    /// </summary>
    public abstract class CachingStrategy : ICachingStrategy
    {
        /// <summary>
        /// Gets the category to which this caching element will be applied.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets the priority at which a item should be entered into a cache, providing
        /// a hint to the cache as to what items can be discarded first if required.
        /// </summary>
        public CacheItemPriority ItemPriority { get; set; } = CacheItemPriority.Medium;

        /// <summary>
        /// Gets the priority of this rule, which is used to distinguish between rules that
        /// match the same type.
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Gets the type name that this caching options element will apply to, which
        /// may include wildcards for mapping.
        /// </summary>
        public string TypeName { get; set; }


        /// <summary>
        /// Gets a value which indicates whether or not this strategy could handle the
        /// specified key / value pair, which is determined by whether or not the
        /// type of the given value is assigned to the type specified in the <see cref="TypeName"/>
        /// property.
        /// </summary>
        /// <param name="category">
        /// The category into which the value is being stored.
        /// </param>
        /// <param name="value">
        /// The value being inserted.
        /// </param>
        /// <returns>
        /// Whether or not this strategy will apply to the given pair.
        /// </returns>
        public bool AppliesTo(string category, object value)
        {
            if (TypeName.Equals("*"))
            {
                return true;
            }

            return CategoryMatches(category) && TypeMatches(value);
        }

        /// <summary>
        /// Gets the options used to store the given key and value pair, returning
        /// CacheOptions.NotCached to indicate the value should not be cached.
        /// </summary>
        /// <returns>
        /// The options used to store the given key-value pair.
        /// </returns>
        public abstract CacheOptions GetOptions();

        private bool CategoryMatches(string category)
        {
            return string.IsNullOrEmpty(Category) || Category.Equals(category, StringComparison.OrdinalIgnoreCase);
        }

        private bool TypeMatches(object value)
        {
            if (string.IsNullOrEmpty(TypeName))
            {
                return true;
            }

            var valueType = value.GetType();

            return valueType.FullName.EndsWith(TypeName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
