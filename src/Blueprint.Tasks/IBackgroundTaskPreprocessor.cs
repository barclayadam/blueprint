namespace Blueprint.Tasks
{
    /// <summary>
    /// A preprocessor that, for a given <see cref="IBackgroundTask" />, can add additional
    /// transient information before it is enqueued.
    /// </summary>
    public interface IBackgroundTaskPreprocessor
    {
        /// <summary>
        /// Processes the given background task.
        /// </summary>
        /// <param name="task">The task to process.</param>
        void Preprocess(BackgroundTaskEnvelope task);
    }
}
