using Newtonsoft.Json;

namespace Blueprint.Api
{
    public class Link
    {
        public string Href { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }
}