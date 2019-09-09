using System;
using System.Threading.Tasks;

namespace Blueprint.Api
{
    public class NullResourceEventRepository : IResourceEventRepository
    {
        public Task<object> GetCurrentDataAsync(string href, Type resourceType)
        {
            return Task.FromResult((object)null);
        }

        public Task AddAsync(ResourceEvent resourceEvent)
        {
            return Task.CompletedTask;
        }
    }
}