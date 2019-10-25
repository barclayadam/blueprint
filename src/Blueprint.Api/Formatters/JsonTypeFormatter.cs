using System.Text;
using System.Threading.Tasks;
using Blueprint.Api.Infrastructure;
using Newtonsoft.Json;

namespace Blueprint.Api.Formatters
{
    public class JsonTypeFormatter : ITypeFormatter
    {
        private static readonly JsonSerializer Serializer = JsonSerializer.Create(JsonApiSerializerSettings.Value);
        private static readonly string JsonContentType = "application/json";

        private readonly IHttpResponseStreamWriterFactory writerFactory;

        public JsonTypeFormatter(IHttpResponseStreamWriterFactory writerFactory)
        {
            this.writerFactory = writerFactory;
        }

        public bool IsSupported(ApiOperationContext context, string format)
        {
            return format == "json";
        }

        public async Task WriteAsync(ApiOperationContext context, string format, object result)
        {
            context.Response.ContentType = JsonContentType;

            using (var httpResponseStreamWriter = writerFactory.CreateWriter(context.Response.Body, Encoding.UTF8))
            using (var jsonWriter = new JsonTextWriter(httpResponseStreamWriter))
            {
                jsonWriter.CloseOutput = false;
                jsonWriter.AutoCompleteOnClose = false;

                Serializer.Serialize(jsonWriter, result);

                // Perf: call FlushAsync to call WriteAsync on the stream with any content left in the TextWriter's
                // buffers. This is better than just letting dispose handle it (which would result in a synchronous write).
                await httpResponseStreamWriter.FlushAsync();
            }
        }
    }
}
