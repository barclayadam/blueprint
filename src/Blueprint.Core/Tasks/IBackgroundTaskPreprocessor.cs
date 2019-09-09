namespace Blueprint.Core.Tasks
{
    public interface IBackgroundTaskPreprocessor<in T> where T : BackgroundTask
    {
        void Preprocess(T task);
    }
}