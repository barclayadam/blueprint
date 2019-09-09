using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Blueprint.Core.Properties;

namespace Blueprint.Core.Caching.Configuration
{
    /// <summary>
    /// Provides a means of exposing simple caching configuration, allowing the
    /// caching of types to be controlled externally and without additional coding
    /// effort.
    /// </summary>
    /// <seealso cref="CachingOptionsElement"/>
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface",
            Justification = "No requirement to implement ICollection")]
    public class CachingConfigurationCollection : ConfigurationElementCollection
    {
        private static readonly Dictionary<string, Func<ConfigurationElement>> SupportedRuleElements =
                new Dictionary<string, Func<ConfigurationElement>>
                {
                        { "sliding", () => new SlidingCachingOptionsElement() },
                        { "fixed", () => new FixedCachingOptionsElement() }
                };

        /// <summary>
        /// Gets the type of this collection, <see cref="ConfigurationElementCollectionType.BasicMap"/>.
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType { get { return ConfigurationElementCollectionType.BasicMap; } }

        /// <summary>
        /// Gets all strategies that have been defined within this configuration collection.
        /// </summary>
        public IEnumerable<ICachingStrategy> Strategies { get { return from CachingOptionsElement rule in this select rule; } }

        /// <summary>
        /// Constructs a new instance of <see cref="CachingOptionsElement"/>.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="CachingOptionsElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            throw new NotImplementedException(
                    Resources.CachingConfigurationCollection_CreateNewElement_NotImplementedException_Message);
        }

        /// <summary>
        /// Creates a new element from a node of the given name, with the name being used to distinguish
        /// between the different available options types (e.g. sliding vs fixed).
        /// </summary>
        /// <param name="elementName">
        /// The element (XML) name.
        /// </param>
        /// <returns>
        /// A new element.
        /// </returns>
        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            return SupportedRuleElements[elementName]();
        }

        /// <summary>
        /// Gets the key of the given configuration element, which should be of type CachingOptionsElement
        /// and is defined as the <see cref="CachingOptionsElement.TypeName"/> property.
        /// </summary>
        /// <param name="element">
        /// The element to return the key for.
        /// </param>
        /// <returns>
        /// The 'element key' of the given element.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CachingOptionsElement)element).TypeName;
        }

        /// <summary>
        /// Indicates whether the given element is valid for this collection, which is it if
        /// the name is one of 'sliding' or 'fixed.
        /// </summary>
        /// <param name="elementName">
        /// The name of the element to check.
        /// </param>
        /// <returns>
        /// Whether the element is valid for this collection.
        /// </returns>
        protected override bool IsElementName(string elementName)
        {
            return SupportedRuleElements.ContainsKey(elementName);
        }
    }
}