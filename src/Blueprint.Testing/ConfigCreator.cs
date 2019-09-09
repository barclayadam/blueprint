using System.Configuration;
using System.IO;

namespace Blueprint.Testing
{
    /// <summary>
    /// A simple helper class containing methods that are able to create <see cref="Configuration"/>
    /// instances in-memory from configuration XML, providing the ability to test configuration
    /// classes.
    /// </summary>
    public static class ConfigCreator
    {
        /// <summary>
        /// Creates a temporary configuration instance given the specified configuration XML, which
        /// should <b>not</b> have the XML declaration, nor the &lt;configuration&gt; node.
        /// </summary>
        /// <param name="configurationXml">The configuration XML used to create the configuration instance.</param>
        /// <returns>The configuration instance, as loaded from the given XML.</returns>
        public static Configuration CreateTemporaryConfiguration(string configurationXml)
        {
            var configurationFile = Path.GetTempFileName();

            File.WriteAllText(
                              configurationFile,
                              "<?xml version='1.0'?><configuration>" + configurationXml + "</configuration>");

            return ConfigurationManager.OpenMappedExeConfiguration(
                                                                   new ExeConfigurationFileMap
                                                                   {
                                                                           ExeConfigFilename = configurationFile
                                                                   },
                                                                   ConfigurationUserLevel.None);
        }
    }
}