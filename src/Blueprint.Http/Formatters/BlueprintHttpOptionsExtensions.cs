using System;
using Blueprint.Http.Formatters;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace To make the extensions discoverable
namespace Blueprint.Http
{
    /// <summary>
    /// A set of extension methods that enable the use of Newtonsoft.JSON instead of the built-in System.Text.Json based
    /// implementations.
    /// </summary>
    public static class BlueprintHttpOptionsExtensions
    {
        /// <summary>
        /// Overrides the default implementation of JSON parsing and formatting to use Newtonsoft.JSON
        /// instead, using default serializer settings.
        /// </summary>
        /// <param name="options">The options to configure.</param>
        public static void UseNewtonsoft(this BlueprintHttpOptions options)
        {
            options.UseNewtonsoft(NewtonsoftJsonResultOutputFormatter.CreateSettings());
        }

        /// <summary>
        /// Overrides the default implementation of JSON parsing and formatting to use Newtonsoft.JSON
        /// instead, using default serializer settings that can be modified by the given action..
        /// </summary>
        /// <param name="options">The options to configure.</param>
        /// <param name="configureSettings">An action called to configure the default settings.</param>
        public static void UseNewtonsoft(this BlueprintHttpOptions options, Action<JsonSerializerSettings> configureSettings)
        {
            var defaultSettings = NewtonsoftJsonResultOutputFormatter.CreateSettings();
            configureSettings(defaultSettings);

            // Inserting these at the start will ensure they are selected first, before the default
            // SystemTextJson implementations.
            options.BodyParsers.Insert(0, new NewtonsoftJsonBodyParser(defaultSettings));
            options.OutputFormatters.Insert(0, new NewtonsoftJsonResultOutputFormatter(defaultSettings));
        }

        /// <summary>
        /// Overrides the default implementation of JSON parsing and formatting to use Newtonsoft.JSON
        /// instead with the given <see cref="JsonSerializerSettings" />.
        /// </summary>
        /// <param name="options">The options to configure.</param>
        /// <param name="settings">The settings to use.</param>
        public static void UseNewtonsoft(this BlueprintHttpOptions options, JsonSerializerSettings settings)
        {
            // Inserting these at the start will ensure they are selected first, before the default
            // SystemTextJson implementations.
            options.BodyParsers.Insert(0, new NewtonsoftJsonBodyParser(settings));
            options.OutputFormatters.Insert(0, new NewtonsoftJsonResultOutputFormatter(settings));
        }
    }
}
