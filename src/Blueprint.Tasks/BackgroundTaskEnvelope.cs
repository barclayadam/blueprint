using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blueprint.Tasks
{
    /// <summary>
    /// Wraps an <see cref="IBackgroundTask" /> to be pushed to the background processing queue, containing
    /// extra details about the execution of the given background task such as system and user metadata.
    /// </summary>
    public class BackgroundTaskEnvelope
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="BackgroundTaskEnvelope" /> for the specified
        /// <see cref="IBackgroundTask" />.
        /// </summary>
        /// <param name="task">The background task being wrapped.</param>
        public BackgroundTaskEnvelope(IBackgroundTask task)
        {
            Task = task;
        }

        /// <summary>
        /// Gets the <see cref="IBackgroundTask" /> this envelope is wrapping.
        /// </summary>
        public IBackgroundTask Task { get; }

        /// <summary>
        /// A simple string dictionary that APM tools use to inject &amp; extract state (i.e. Spans from
        /// OpenTracing) to enable cross-process propagation.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IDictionary<string, string> ApmContext { get; set; }
    }
}
