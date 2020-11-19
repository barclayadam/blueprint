using Newtonsoft.Json;

namespace Blueprint.Tasks
{
    /// <summary>
    /// A wrapper around <see cref="BackgroundTaskEnvelope" /> that is used to provide a slighter
    /// nicer dashboard experience by giving a nicer <see cref="ToString" /> implementation which
    /// gives details of the actual task without metadata.
    /// </summary>
    public class HangfireBackgroundTaskWrapper
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="HangfireBackgroundTaskWrapper" /> class.
        /// </summary>
        /// <param name="envelope">The task envelope to wrap.</param>
        public HangfireBackgroundTaskWrapper(BackgroundTaskEnvelope envelope)
        {
            this.Envelope = envelope;
        }

        /// <summary>
        /// The wrapped envelope.
        /// </summary>
        public BackgroundTaskEnvelope Envelope { get; private set; }

        /// <summary>
        /// Converts the task into a JSON representation as: <c>TaskTypeName</c>([<c>JSON without {}</c>]).
        /// </summary>
        /// <returns>A string representation of the task.</returns>
        public override string ToString()
        {
            // Extra safety if, for some reason, deserialisation fails. We do not want this to blow up
            // on a single job as it could mean not being able to view jobs at all in the Hangfire dashboard.
            if (this.Envelope?.Task == null)
            {
                return "[ERR] Unknown task";
            }

            // A slightly more useful default for Hangfire to not
            // include the type's namespace
            return $"{this.Envelope.Task.GetType().Name}({this.GetParamDisplay()})";
        }

        private string GetParamDisplay()
        {
            // Parameters displayed as JSON but without the opening and closing parens
            // to slightly aid readability
            var parameters = JsonConvert.SerializeObject(this.Envelope.Task);

            return parameters.Substring(1, parameters.Length - 2);
        }
    }
}
