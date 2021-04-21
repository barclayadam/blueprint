using System;
using System.Threading.Tasks;

namespace Blueprint.Http
{
    /// <summary>
    /// An <see cref="IResourceEventRepository" /> that will do nothing.
    /// </summary>
    public class NullResourceEventRepository : IResourceEventRepository
    {
        /// <inheritdoc/>
        public Task<object> GetCurrentDataAsync(string href, Type resourceType)
        {
            return Task.FromResult((object)null);
        }

        /// <inheritdoc/>
        public Task AddAsync(ResourceEvent resourceEvent)
        {
            return Task.CompletedTask;
        }
    }
}
