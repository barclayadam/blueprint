using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Tasks.Provider;

namespace Blueprint.Tasks
{
    /// <summary>
    /// Represents the "store" of a running task, providing the ability to store arbitrary pieces of
    /// data for a task (based only on task type, so shared across all runs no matter the properties).
    /// </summary>
    public class BackgroundTaskStore
    {
        private readonly string contextKey;
        private readonly IBackgroundTaskContextProvider provider;

        private bool loaded;
        private bool modified;

        // NB: We only create this on first modification, or set it on first load to avoid allocating
        // empty dictionaries for every creation of context when most tasks do not require this
        private BackgroundTaskContextData data;

        /// <summary>
        /// Initialises a new instance of the <see cref="BackgroundTaskStore" /> class.
        /// </summary>
        /// <param name="contextKey">The "key" of this store.</param>
        /// <param name="provider">The provider that implements the actual persistent storage.</param>
        public BackgroundTaskStore(string contextKey, IBackgroundTaskContextProvider provider)
        {
            this.contextKey = contextKey;
            this.provider = provider;
        }

        /// <summary>
        /// Gets the key of this context, a value that identifies the context in the system. This value will
        /// NOT be unique per execution but is instead usually shared between task <b>types</b>.
        /// </summary>
        public string Key => contextKey;

        /// <summary>
        /// Creates a new 'empty' <see cref="BackgroundTaskStore"/> which will not have any data that
        /// can be loaded and uses an <see cref="InMemoryBackgroundTaskContextProvider"/>.
        /// </summary>
        /// <returns>A new 'empty' context.</returns>
        public static BackgroundTaskStore Empty()
        {
            return new BackgroundTaskStore("Empty", new InMemoryBackgroundTaskContextProvider());
        }

        /// <summary>
        /// Creates a new <see cref="BackgroundTaskStore"/> that will be able to get data from the given dictionary, and uses
        /// an <see cref="InMemoryBackgroundTaskContextProvider"/>.
        /// </summary>
        /// <param name="data">The data dictionary to set for the created context.</param>
        /// <returns>A new 'empty' context.</returns>
        public static BackgroundTaskStore WithData(Dictionary<string, object> data)
        {
            return new BackgroundTaskStore("Explicit", new InMemoryBackgroundTaskContextProvider(data));
        }

        /// <summary>
        /// Gets a value that has previously been stored in this context, loading data on-demand to avoid overhead for
        /// every task that may not have any data (i.e. do not pre-load as most would be empty operations).
        /// </summary>
        /// <param name="key">The key of the item to be loaded.</param>
        /// <typeparam name="T">The type of the value that has been stored.</typeparam>
        /// <returns>The stored value, or <c>default(T)</c> if it does not exist.</returns>
        public async Task<T> GetAsync<T>(string key)
        {
            if (!loaded)
            {
                var loadedData = await provider.LoadDataAsync(contextKey)
                                 ?? new BackgroundTaskContextData(contextKey);

                // If data has been modified before loading we need to merge those
                // changes in to the existing data
                if (modified)
                {
                    foreach (var modifiedKvp in data.Data)
                    {
                        loadedData.Data[modifiedKvp.Key] = modifiedKvp.Value;
                    }
                }

                data = loadedData;
                loaded = true;
            }

            if (data.Data.TryGetValue(key, out var value))
            {
                return (T)value;
            }

            return default;
        }

        /// <summary>
        /// Sets a value for the given key, which will be persisted on completion of the task this context is for and be
        /// available through the <see cref="GetAsync{T}"/> method immediately and on subsequent runs.
        /// </summary>
        /// <param name="key">The non-null key of the item to set.</param>
        /// <param name="value">The value to store.</param>
        public void Set(string key, object value)
        {
            modified = true;

            if (data == null)
            {
                data = new BackgroundTaskContextData(contextKey);
            }

            data.Data[key] = value;
        }

        /// <summary>
        /// Saves any changes that have been made to the data of this context, using the supplied <see cref="IBackgroundTaskContextProvider" />.
        /// </summary>
        /// <returns>A task representing the save operation.</returns>
        public Task SaveAsync()
        {
            if (!modified)
            {
                return Task.CompletedTask;
            }

            return provider.SaveDataAsync(data);
        }
    }
}
