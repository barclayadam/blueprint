using Newtonsoft.Json;

namespace Blueprint.Core.Tasks
{
    using System.Collections.Generic;

    /// <summary>
    /// Contains metadata about a task that is to be executed / scheduled, containing information like
    /// who is executing the task, where it comes from etc. to enable better diagnostics of
    /// executed tasks.
    /// </summary>
    public class BackgroundTaskMetadata
    {
        /// <summary>
        /// Gets or sets the request id associated with the code that triggered the associated
        /// background task.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string RequestId { get; set; }

        /// <summary>
        /// Gets or sets the request 'baggage' that is used to pass around context data about requests through
        /// distributed systems.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<KeyValuePair<string, string>> RequestBaggage { get; set; }

        /// <summary>
        /// Gets or sets the source system that triggered the associated background task.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string System { get; set; }

        /// <summary>
        /// Gets or sets the source system that triggered the associated background task.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SystemVersion { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user that triggered the associated background task.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string InitiatingUser { get; set; }
    }
}