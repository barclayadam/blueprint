using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blueprint.Core.Tasks
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

        public Task<BackgroundTaskContextDataItem> GetDataAsync(string contextKey)
        {
            return Task.FromResult(new BackgroundTaskContextDataItem(contextKey, data));
        }

        public Task SetDataAsync(BackgroundTaskContextDataItem data)
        {
            throw new NotImplementedException("InMemoryBackgroundTaskContextProvider is read-only");
        }
    }
}
