using System;
using System.Configuration;

namespace Blueprint.Core.Caching.Configuration
{
    /// <summary>
    /// A caching options element which defines the options for a type (or collection
    /// of using regular expressions).
    /// </summary>
    public abstract class CachingOptionsElement : ConfigurationElement, ICachingStrategy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachingOptionsElement"/> class.
        /// </summary>
        internal CachingOptionsElement()
        {
        }

        /// <summary>
        /// Gets the category to which this caching element will be applied.
        /// </summary>
        [ConfigurationProperty("category", IsRequired = false)]
        public string Category { get { return (string)this["category"]; } }

        /// <summary>
        /// Gets the priority at which a item should be entered into a cache, providing
        /// a hint to the cache as to what items can be discarded first if required.
        /// </summary>
        [ConfigurationProperty("itemPriority", IsRequired = false, DefaultValue = CacheItemPriority.Medium)]
        public CacheItemPriority ItemPriority { get { return (CacheItemPriority)this["itemPriority"]; } }

        /// <summary>
        /// Gets the priority of this rule, which is used to distinguish between rules that
        /// match the same type.
        /// </summary>
        [ConfigurationProperty("rulePriority", IsRequired = false, DefaultValue = 0)]
        public int Priority { get { return (int)this["rulePriority"]; } }

        /// <summary>
        /// Gets the type name that this caching options element will apply to, which
        /// may include wildcards for mapping.
        /// </summary>
        [ConfigurationProperty("type", IsRequired = false)]
        public string TypeName
        {
            get
            {
                var type = (string)this["type"];

                return type;
            }
        }

        /// <summary>
        /// Gets a value which indicates whether or not this strategy could handle the 
        /// specified key / value pair, which is determined by whether or not the
        /// type of the given value is assigned to the type specified in the <see cref="TypeName"/>
        /// property.
        /// </summary>
        /// <param name="category">
        /// The category intoo which the value is being stored.
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