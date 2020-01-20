using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Blueprint.Api
{
    public static class JsonApiSerializerSettings
    {
        public static readonly JsonSerializerSettings Value = Create();

        public static JsonSerializerSettings Create()
        {
            var settings = new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.DateTimeOffset,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),

                // Do not change this setting
                // Setting this to None prevents Json.NET from loading malicious, unsafe, or security-sensitive types
                TypeNameHandling = TypeNameHandling.None,
            };

            settings.Converters.Add(new IsoDateTimeConverter());
            settings.Converters.Add(new HttpMethodConverter());
            settings.Converters.Add(new StringEnumConverter());
            settings.Converters.Add(new Base64FileConverter());

            return settings;
        }
    }
}
