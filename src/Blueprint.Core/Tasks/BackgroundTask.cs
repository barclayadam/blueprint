using Newtonsoft.Json;

namespace Blueprint.Core.Tasks
{
    /// <summary>
    /// Represents a task, a unit of work that is typically executed out of process, such
    /// that immediate execution is not required.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An <see cref="BackgroundTask"/> is implemented as two parts, the task definition which is
    /// a simple data-bag class that implements this marker interface, plus an implementation
    /// of an <see cref="IBackgroundTaskHandler{TTask}"/> that performs the actual execution given
    /// a task definition.
    /// </para>
    /// <para>
    /// To execute / schedule a background task use <see cref="IBackgroundTaskScheduler"/>.
    /// </para>
    /// </remarks>
    public class BackgroundTask
    {
        /// <summary>
        /// Initialises a new instance of <see cref="BackgroundTask" />, setting <see cref="Metadata"/> to a new
        /// <see cref="BackgroundTaskMetadata" /> with it's <see cref="BackgroundTaskMetadata.RequestId" /> property
        /// set to <see cref="RequestIdAccessor.Id" />.
        /// </summary>
        public BackgroundTask()
        {
            Metadata = new BackgroundTaskMetadata();
        }

        /// <summary>
        /// Gets or sets the envelope of this background task that helps to identify metadata about
        /// </summary>
        public BackgroundTaskMetadata Metadata { get; set; }

        public override string ToString()
        {
            // A slightly more useful default for Hangfire to not
            // include the type's namespace
            return $"{GetType().Name}({GetParamDisplay()})";
        }

        protected virtual string GetParamDisplay()
        {
            // Parameters displayed as JSON but without the opening and closing parens
            // to slightly aid readability
            var parameters = JsonConvert.SerializeObject(this);

            return parameters.Substring(1, parameters.Length - 2);
        }
    }
}
