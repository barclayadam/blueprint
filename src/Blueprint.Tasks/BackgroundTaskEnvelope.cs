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
            Metadata = new BackgroundTaskMetadata();
        }

        /// <summary>
        /// Gets the <see cref="IBackgroundTask" /> this envelope is wrapping.
        /// </summary>
        public IBackgroundTask Task { get; }

        /// <summary>
        /// Gets or sets the envelope of this background task that helps to identify metadata about.
        /// </summary>
        public BackgroundTaskMetadata Metadata { get; set; }
    }
}
