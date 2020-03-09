using Newtonsoft.Json;

namespace Blueprint.Tasks
{
    public class HangfireBackgroundTaskWrapper
    {
        public BackgroundTaskEnvelope Envelope { get; }

        public HangfireBackgroundTaskWrapper(BackgroundTaskEnvelope envelope)
        {
            Envelope = envelope;
        }

        public override string ToString()
        {
            // A slightly more useful default for Hangfire to not
            // include the type's namespace
            return $"{Envelope.BackgroundTask.GetType().Name}({GetParamDisplay()})";
        }

        private string GetParamDisplay()
        {
            // Parameters displayed as JSON but without the opening and closing parens
            // to slightly aid readability
            var parameters = JsonConvert.SerializeObject(Envelope.BackgroundTask);

            return parameters.Substring(1, parameters.Length - 2);
        }
    }
}