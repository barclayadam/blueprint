using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blueprint.Core.Tasks
{
    public class BackgroundTaskContextDataItem
    {
        public BackgroundTaskContextDataItem(string contextKey)
        {
            ContextKey = contextKey;
            Data = new Dictionary<string, object>();
        }

        [JsonConstructor]
        public BackgroundTaskContextDataItem(string contextKey, Dictionary<string, object> data)
        {
            ContextKey = contextKey;
            Data = data;
        }

        public string ContextKey { get; set; }

        public Dictionary<string, object> Data { get; set; }
    }
}
