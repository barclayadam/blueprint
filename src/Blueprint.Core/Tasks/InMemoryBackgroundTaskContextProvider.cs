namespace Blueprint.Core.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class InMemoryBackgroundTaskContextProvider : IBackgroundTaskContextProvider
    {
        private readonly Dictionary<string, object> data;

        public InMemoryBackgroundTaskContextProvider(Dictionary<string,object> data = null)
        {
            this.data = data ?? new Dictionary<string, object>();
        }

        public Task<BackgroundTaskContextDataItem> GetDataAsync(string contextKey)
        {
            return Task.FromResult(new BackgroundTaskContextDataItem(contextKey, this.data));
        }

        public Task SetDataAsync(BackgroundTaskContextDataItem data)
        {
            throw new NotImplementedException("InMemoryBackgroundTaskContextProvider is read-only");
        }
    }
}