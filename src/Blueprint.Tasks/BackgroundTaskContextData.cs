using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blueprint.Tasks
{
    public class BackgroundTaskContextData
    {
        public BackgroundTaskContextData(string contextKey)
        {
            ContextKey = contextKey;
            Data = new Dictionary<string, object>();
        }

        [JsonConstructor]
        public BackgroundTaskContextData(string contextKey, Dictionary<string, object> data)
        {
            ContextKey = contextKey;
            Data = data;
        }

        public string ContextKey { get; set; }

        public Dictionary<string, object> Data { get; set; }
    }
}
