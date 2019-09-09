using System;
using System.Collections.Generic;

namespace Blueprint.Core.Caching.Configuration
{
    /// <summary>
    /// Represents the configuration section that is used to configure caching.
    /// </summary>
    /// <remarks>
    /// Configuration of caching is done through a series of rules that allow different types
    /// of cached objects to be cached for differing periods of time, or not at all. The rules
    /// are defined as a list of sliding / fixed rules that define the options that will
    /// be used to construct the <see cref="CacheOptions"/> when caching an object.
    /// </remarks>
    /// <example>
    /// The below example shows the pertinent parts required to integrate the caching
    /// configuration into an application configuration file.
    /// <code lang="xml">
    ///    <configuration>
    ///      <configSections>
    ///        <section name="caching" type="Blueprint.Core.Caching.Configuration.CachingConfiguration, Blueprint.Core"/>
    ///      </configSections>
    ///      <caching enabled="true" provider="Blueprint.Core.Caching.WebCache, Blueprint.Core">
    ///        <rules>
    ///            <sliding type="*" timeSpan="20:00:00" itemPriority="Low" rulePriority="-1000"/>
    ///
    ///            <sliding type="QueryResult1" timeSpan="5:00:00"/>
    ///            <fixed category="Content" timeSpan="45:00:00"/>
    ///        </rules>
    ///      </caching>
    ///    </configuration>
    /// </code>
    /// </example>
    public class CachingConfiguration
    {
        /// <summary>
        /// Gets or sets the current configuration of the caching components, as retrieved from the
        /// current <see cref="CachingConfiguration"/>.
        /// </summary>
        /// <remarks>
        /// If this value is set to a non-null value that will be used instead of the configuration loaded
        /// from the app or web config files, to be used mainly for testing purposes.
        /// </remarks>
        public static CachingConfiguration Current { get; set; } = new CachingConfiguration();

        /// <summary>
        /// Gets a value indicating whether caching should be enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets the type of the provider that should be used for caching.
        /// </summary>
        public Type ProviderType { get; set; } = typeof(NoCacheProvider);

        /// <summary>
        /// Gets the strategies that have been defined, used to decide how (and if) an item will be cached;
        /// </summary>
        public List<ICachingStrategy> Strategies { get; set; } = new List<ICachingStrategy>();
    }
}
