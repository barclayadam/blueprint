using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blueprint.Tasks
{
    /// <summary>
    /// Holds data that can be persisted across executions of <see cref="IBackgroundTask" />s.
    /// </summary>
    public class BackgroundTaskContextData
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="BackgroundTaskContextData" /> class with
        /// an empty data dictionary.
        /// </summary>
        /// <param name="contextKey">The key for this data.</param>
        public BackgroundTaskContextData(string contextKey)
        {
            this.ContextKey = contextKey;
            this.Data = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="BackgroundTaskContextData" /> class with
        /// an the specified data dictionary.
        /// </summary>
        /// <param name="contextKey">The key for this data.</param>
        /// <param name="data">The data.</param>
        [JsonConstructor]
        public BackgroundTaskContextData(string contextKey, Dictionary<string, object> data)
        {
            this.ContextKey = contextKey;
            this.Data = data;
        }

        /// <summary>
        /// The identifying key of this data.
        /// </summary>
        public string ContextKey { get; set; }

        /// <summary>
        /// The data to be stored.
        /// </summary>
        public Dictionary<string, object> Data { get; set; }
    }
}
