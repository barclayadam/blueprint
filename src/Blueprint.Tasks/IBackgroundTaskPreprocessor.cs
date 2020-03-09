namespace Blueprint.Tasks
{
    public interface IBackgroundTaskPreprocessor<in T> where T : IBackgroundTask
    {
        void Preprocess(T task);
    }
}
