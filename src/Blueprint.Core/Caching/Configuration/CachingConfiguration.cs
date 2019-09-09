using System;
using System.Configuration;
using Blueprint.Core.ThirdParty;

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
    [UsedImplicitly]
    public class CachingConfiguration : ConfigurationSection
    {
        private static CachingConfiguration currentConfiguration;

        /// <summary>
        /// Gets or sets the current configuration of the caching components, as retrieved from the
        /// current <see cref="CachingConfiguration"/>.
        /// </summary>
        /// <remarks>
        /// If this value is set to a non-null value that will be used instead of the configuration loaded
        /// from the app or web config files, to be used mainly for testing purposes.
        /// </remarks>
        public static CachingConfiguration Current
        {
            get { return currentConfiguration ?? (ConfigurationManager.GetSection("caching") as CachingConfiguration) ?? new CachingConfiguration(); }

            set { currentConfiguration = value; }
        }

        /// <summary>
        /// Gets a value indicating whether caching should be enabled.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        public bool IsEnabled { get { return (bool)this["enabled"]; } }

        /// <summary>
        /// Gets the type of the provider that should be used for caching.
        /// </summary>
        public Type ProviderType { get { return Type.GetType(Provider); } }

        /// <summary>
        /// Gets the priority at which a item should be entered into a cache, providing
        /// a hint to the cache as to what items can be discarded first if required.
        /// </summary>
        [ConfigurationProperty("rules", IsRequired = false, IsDefaultCollection = true)]
        public CachingConfigurationCollection Rules { get { return (CachingConfigurationCollection)this["rules"]; } }

        /// <summary>
        /// Gets the type of the provider that should be used for caching.
        /// </summary>
        [ConfigurationProperty("provider", IsRequired = false, DefaultValue = "Blueprint.Core.Caching.NoCacheProvider, Blueprint.Core")]
        private string Provider { get { return (string)this["provider"]; } }

        /// <summary>
        /// Indicates whether or not to allow an unrecognized attribute, in this case allowing only
        /// the xmlns attribute to be declared to get intellisense to work.
        /// </summary>
        /// <param name="name">
        /// The name of the unrecognised attrinute.
        /// </param>
        /// <param name="value">
        /// The value of the unrecognised attrinute.
        /// </param>
        /// <returns>
        /// Whether ot allow the unrecognised attribute.
        /// </returns>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            return name == "xmlns";
        }
    }
}