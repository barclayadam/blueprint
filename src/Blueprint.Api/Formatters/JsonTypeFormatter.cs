using System.Text.Json;
using System.Threading.Tasks;

namespace Blueprint.Api.Formatters
{
    public class JsonTypeFormatter : ITypeFormatter
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private static readonly string JsonContentType = "application/json";

        public bool IsSupported(ApiOperationContext context, string format)
        {
            return format == "json";
        }

        public async Task WriteAsync(ApiOperationContext context, string format, object result)
        {
            context.Response.ContentType = JsonContentType;

            await JsonSerializer.SerializeAsync(context.Response.Body, result, result.GetType(), JsonSerializerOptions);
            await context.Response.Body.FlushAsync();
        }
    }
}
