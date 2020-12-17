using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blueprint.Http
{
    /// <summary>
    /// Configuration options for the System.Text.Json JSON implementation, allowing for the
    /// configuration of the <see cref="JsonSerializerOptions" /> that will be used.
    /// </summary>
    public class BlueprintJsonOptions
    {
        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,

                PropertyNameCaseInsensitive = true,

                // Use camel casing for properties and dictionaries (dictionaries because
                // we treat them similar to properties from perspective of client consumption)
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,

                // Web defaults don't use the relex JSON escaping encoder.
                //
                // Because these options are for producing content that is written directly to the request
                // (and not embedded in an HTML page for example), we can use UnsafeRelaxedJsonEscaping.
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            options.Converters.Add(new JsonStringEnumConverter());

            return options;
        }

        internal static readonly JsonSerializerOptions DefaultSerializerOptions = CreateOptions();

        /// <summary>
        /// The <see cref="JsonSerializerOptions" /> that will be used, exposed to enable modification
        /// of the options.
        /// </summary>
        // Use a copy so the defaults are not modified.
        public JsonSerializerOptions SerializerOptions { get; } = CreateOptions();
    }
}
