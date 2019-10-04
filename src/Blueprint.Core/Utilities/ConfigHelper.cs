using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Blueprint.Core.Utilities
{
    public static class ConfigHelper
    {
        /// <summary>
        /// Gets or sets the .NET Core <see cref="IConfigurationRoot" /> in the case that we are running in core, using this
        /// instead of ConfigurationManager to load settings.
        /// </summary>
        public static IConfigurationRoot Configuration { get; set; }

        public static string GetConfigValue(this string key, IfEmpty ifEmpty = IfEmpty.DefaultValue)
        {
            Guard.NotNullOrEmpty(nameof(key), key);

            return GetConfigValue<string>(key, ifEmpty);
        }

        public static T GetConfigValue<T>(this string key, IfEmpty ifEmpty = IfEmpty.DefaultValue)
        {
            Guard.NotNullOrEmpty(nameof(key), key);

            if (TryGetAppSetting(key, out T value))
            {
                return value;
            }

            if (ifEmpty == IfEmpty.ShouldThrow)
            {
                throw new ConfigurationErrorsException($"Missing '{key}' from configuration");
            }

            return value;
        }

        public static string GetConnectionString(this string name)
        {
            Guard.NotNullOrEmpty(nameof(name), name);

            name = $"connection:{name}";

            if (TryGetAppSetting(name, out string value))
            {
                return value;
            }

            throw new ConfigurationErrorsException($"Missing '{name}' from configuration");
        }

        public static bool TryGetAppSetting<T>(this string key, out T value)
        {
            Guard.NotNullOrEmpty("key", key);

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));

            if (!typeConverter.CanConvertFrom(typeof(string)))
            {
                throw new InvalidOperationException(
                    $"Cannot get app setting of type {typeof(T)} as it cannot be converted from string.");
            }

            if (Configuration == null)
            {
                throw new InvalidOperationException("Must set ConfigHelper.Configuration at app startup");
            }

            var appSettingString = Configuration.GetSection(key).Value;

            if (appSettingString == null)
            {
                value = default;
                return false;
            }

            try
            {
                value = (T)typeConverter.ConvertFrom(appSettingString);
                return true;
            }
            catch (Exception ex)
            {
                // Although this is a 'try' method we will throw an exception if the value actually
                // exists but we could not convert it as it represents a mis-configuration, not
                // a missing value.
                throw new ConfigurationErrorsException(
                    $"Configuration value '{appSettingString}' for '{key}' failed to be converted to type '{typeof(T).Name}'.", ex);
            }
        }

        public static void Set(Dictionary<string, string> dictionary)
        {
            throw new NotImplementedException();
        }
    }
}
