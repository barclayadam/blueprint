namespace Blueprint.Core.Tasks
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class BackgroundTaskContextDataItem
    {
        public BackgroundTaskContextDataItem(string contextKey)
        {
            this.ContextKey = contextKey;
            this.Data = new Dictionary<string, object>();
        }

        [JsonConstructor]
        public BackgroundTaskContextDataItem(string contextKey, Dictionary<string, object> data)
        {
            this.ContextKey = contextKey;
            this.Data = data;
        }

        public string ContextKey { get; set; }

        public Dictionary<string, object> Data { get; set; }
    }
}