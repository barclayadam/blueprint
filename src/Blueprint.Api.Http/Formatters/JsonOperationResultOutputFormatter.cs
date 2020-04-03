using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Api.Http.Formatters
{
    public class JsonOperationResultOutputFormatter : IOperationResultOutputFormatter
    {
        internal static readonly JsonSerializerOptions JsonSerializerOptions = CreateOptions();

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,

                // Use camel casing for properties and dictionaries (dictionaries because
                // we treat them similar to properties from perspective of client consumption)
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            };

            options.Converters.Add(new JsonStringEnumConverter());

            return options;
        }

        private static readonly string JsonContentType = "application/json";

        public bool IsSupported(HttpRequest request, string format)
        {
            return format == "json";
        }

        public async Task WriteAsync(HttpResponse response, string format, object result)
        {
            response.ContentType = JsonContentType;

            await JsonSerializer.SerializeAsync(response.Body, result, result.GetType(), JsonSerializerOptions);
            await response.Body.FlushAsync();
        }
    }
}
