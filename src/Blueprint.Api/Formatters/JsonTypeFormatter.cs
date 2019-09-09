using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace Blueprint.Api.Formatters
{
    public class JsonTypeFormatter : ITypeFormatter
    {
        private static readonly JsonSerializer Serializer = JsonSerializer.Create(JsonApiSerializerSettings.Value);

        private static readonly string JsonContentType = "application/json";

        public bool IsSupported(ApiOperationContext context, string format)
        {
            return format == "json";
        }

        public void Write(ApiOperationContext context, string format, object result)
        {
            context.Response.ContentType = JsonContentType;
            
            using (var httpResponseStreamWriter = new HttpResponseStreamWriter(context.Response.Body, Encoding.UTF8))
            using (var jsonWriter = new JsonTextWriter(httpResponseStreamWriter))
            {
                jsonWriter.CloseOutput = false;
                jsonWriter.AutoCompleteOnClose = false;
                
                Serializer.Serialize(jsonWriter, result);
            }
        }
    }
}
