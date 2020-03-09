using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Core;
using Blueprint.Tasks.Provider;

namespace Blueprint.Tasks
{
    public class InMemoryBackgroundTaskContextProvider : IBackgroundTaskContextProvider
    {
        private readonly Dictionary<string, object> data;

        public InMemoryBackgroundTaskContextProvider()
        {
            data = new Dictionary<string, object>();
        }

        public InMemoryBackgroundTaskContextProvider(Dictionary<string, object> data)
        {
            Guard.NotNull(nameof(data), data);

            this.data = data;
        }

        public Task<BackgroundTaskContextData> LoadDataAsync(string contextKey)
        {
            return Task.FromResult(new BackgroundTaskContextData(contextKey, data));
        }

        public Task SaveDataAsync(BackgroundTaskContextData data)
        {
            throw new NotImplementedException("InMemoryBackgroundTaskContextProvider is read-only");
        }
    }
}
