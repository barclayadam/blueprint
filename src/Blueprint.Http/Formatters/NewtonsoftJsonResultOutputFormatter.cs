using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Blueprint.Http.Formatters
{
    /// <summary>
    /// An <see cref="IOperationResultOutputFormatter" /> that will use System.Text.Json to
    /// format responses as JSON.
    /// </summary>
    public class NewtonsoftJsonResultOutputFormatter : IOperationResultOutputFormatter
    {
        private readonly JsonSerializer _bodyJsonSerializer;
        private readonly List<MediaType> _supportedMediaTypes;

        /// <summary>
        /// Initialises a new instance of the <see cref="NewtonsoftJsonResultOutputFormatter" /> class.
        /// </summary>
        /// <param name="settings">The JSON serializer settings.</param>
        public NewtonsoftJsonResultOutputFormatter(JsonSerializerSettings settings)
        {
            this._bodyJsonSerializer = JsonSerializer.Create(settings);

            this._supportedMediaTypes = new List<MediaType>
            {
                new MediaType(MediaTypeHeaderValues.ApplicationJson.ToString()),
                new MediaType(MediaTypeHeaderValues.TextJson.ToString()),
                new MediaType(MediaTypeHeaderValues.ApplicationAnyJsonSyntax.ToString()),
            };
        }

        /// <inheritdoc />
        public bool IsSupported(OutputFormatterCanWriteContext context)
        {
            if (!context.ContentType.HasValue)
            {
                // application/json
                context.ContentType = this._supportedMediaTypes.First();
                return true;
            }

            foreach (var s in this._supportedMediaTypes)
            {
                if (context.ContentType.Value.IsSubsetOf(s))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public async Task WriteAsync(OutputFormatterCanWriteContext context)
        {
            // We know ContentType has been set as we would have set to a default above in IsSupported
            context.Response.ContentType = context.ContentType.Value.ToString();

            var responseStream = context.Response.Body;

            var sb = new StringBuilder(256);
            var sw = new StringWriter(sb, CultureInfo.InvariantCulture);

            using var jsonWriter = new JsonTextWriter(sw)
            {
                Formatting = this._bodyJsonSerializer.Formatting,
            };

            this._bodyJsonSerializer.Serialize(jsonWriter, context.Result, null);

            var jsonBytes = Encoding.UTF8.GetBytes(sw.ToString());

            await responseStream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
            await responseStream.FlushAsync();
        }

        internal static JsonSerializerSettings CreateSettings()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            settings.Converters.Add(new StringEnumConverter());

            return settings;
        }
    }
}
