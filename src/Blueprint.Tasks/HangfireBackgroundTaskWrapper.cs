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
            Envelope = envelope;
        }

        /// <summary>
        /// The wrapped envelope.
        /// </summary>
        public BackgroundTaskEnvelope Envelope { get; private set; }

        /// <summary>
        /// Converts the task into a JSON representation as: [<c>TaskName</c>]([<c>JSON without {}</c>]).
        /// </summary>
        /// <returns>A string representation of the task.</returns>
        public override string ToString()
        {
            // A slightly more useful default for Hangfire to not
            // include the type's namespace
            return $"{Envelope.Task.GetType().Name}({GetParamDisplay()})";
        }

        private string GetParamDisplay()
        {
            // Parameters displayed as JSON but without the opening and closing parens
            // to slightly aid readability
            var parameters = JsonConvert.SerializeObject(Envelope.Task);

            return parameters.Substring(1, parameters.Length - 2);
        }
    }
}
