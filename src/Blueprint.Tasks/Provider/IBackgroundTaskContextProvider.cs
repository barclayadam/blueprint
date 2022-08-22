using System.Threading.Tasks;

namespace Blueprint.Tasks.Provider;

/// <summary>
/// Represents a provider that can save and load data used to provide task related, unstructured
/// data for a <see cref="BackgroundTaskStore" />.
/// </summary>
public interface IBackgroundTaskContextProvider
{
    /// <summary>
    /// Given a key representing the task context gets all previously saved data that is available.
    /// </summary>
    /// <param name="contextKey">The key of the data to load.</param>
    /// <returns>A task resulting in a dictionary of previously saved data, which may be empty.</returns>
    Task<BackgroundTaskContextData> LoadDataAsync(string contextKey);

    /// <summary>
    /// Saves the given context data item to the persistent store.
    /// </summary>
    /// <param name="data">The data to be stored.</param>
    /// <returns>A <see cref="Task"/> representing the async execution.</returns>
    Task SaveDataAsync(BackgroundTaskContextData data);
}