using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Core;
using Blueprint.Tasks.Provider;

namespace Blueprint.Tasks
{
    /// <summary>
    /// An in-memory <see cref="IBackgroundTaskContextProvider" /> that is useful for testing
    /// purposes.
    /// </summary>
    public class InMemoryBackgroundTaskContextProvider : IBackgroundTaskContextProvider
    {
        private readonly Dictionary<string, object> data;

        /// <summary>
        /// Initialises a new instance of the <see cref="InMemoryBackgroundTaskContextProvider" />
        /// class with an empty dictionary of data.
        /// </summary>
        public InMemoryBackgroundTaskContextProvider()
        {
            data = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="InMemoryBackgroundTaskContextProvider" /> class
        /// with the given initial data dictionary that will be returned for all calls
        /// of <see cref="LoadDataAsync" />.
        /// </summary>
        /// <param name="data">The data.</param>
        public InMemoryBackgroundTaskContextProvider(Dictionary<string, object> data)
        {
            Guard.NotNull(nameof(data), data);

            this.data = data;
        }

        /// <summary>
        /// Returns a new <see cref="BackgroundTaskContextData" /> with the data that was
        /// passed in to the constructor of this instance.
        /// </summary>
        /// <param name="contextKey">The context key, ignored.</param>
        /// <returns>A new <see cref="BackgroundTaskContextData" /> with data from this instance.</returns>
        public Task<BackgroundTaskContextData> LoadDataAsync(string contextKey)
        {
            return Task.FromResult(new BackgroundTaskContextData(contextKey, data));
        }

        /// <inheritdoc />
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public Task SaveDataAsync(BackgroundTaskContextData data)
        {
            throw new NotImplementedException("InMemoryBackgroundTaskContextProvider is read-only");
        }
    }
}
