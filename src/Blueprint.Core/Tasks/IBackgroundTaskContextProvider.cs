namespace Blueprint.Core.Tasks
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a provider that can save and load data used to provide task related, unstructured
    /// data for a <see cref="BackgroundTaskContext" />.
    /// </summary>
    public interface IBackgroundTaskContextProvider
    {
        /// <summary>
        /// Given a key representing the task context gets all previously saved data that is available.
        /// </summary>
        /// <param name="contextKey">The key of the data to load.</param>
        /// <returns>A task resulting in a dictionary of previously saved data, which may be empty.</returns>
        Task<BackgroundTaskContextDataItem> GetDataAsync(string contextKey);

        Task SetDataAsync(BackgroundTaskContextDataItem data);
    }
}