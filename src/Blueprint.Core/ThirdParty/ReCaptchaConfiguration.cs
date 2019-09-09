using System.Configuration;

namespace Blueprint.Core.ThirdParty
{
    /// <summary>
    /// Represents the configuration section that is used to configure ReCaptcha assemblies.
    /// </summary>
    /// <example>
    /// <configuration>
    ///   <configSections>
    ///     <section name="reCaptcha" type="Blueprint.Core.ThirdParty.ReCaptchaConfiguration, Blueprint.Core"/>
    ///   </configSections> 
    ///   <reCaptcha privateKey="000000000000000000000" publicKey="00000000000000000000000000"/>
    /// </configuration>
    /// </example>
    [UsedImplicitly]
    public class ReCaptchaConfiguration : ConfigurationSection
    {
        /// <summary>
        /// Gets the current configuration of the ReCaptcha component.
        /// </summary>
        public static ReCaptchaConfiguration Current =>
            (ConfigurationManager.GetSection("reCaptcha") as ReCaptchaConfiguration)
            ?? new ReCaptchaConfiguration();

        /// <summary>
        /// Gets the private key.
        /// </summary>
        [ConfigurationProperty("privateKey", IsRequired = true)]
        public string PrivateKey { get { return (string)this["privateKey"]; } }

        /// <summary>
        /// Gets the public key.
        /// </summary>
        [ConfigurationProperty("publicKey", IsRequired = true)]
        public string PublicKey { get { return (string)this["publicKey"]; } }
    }
}
