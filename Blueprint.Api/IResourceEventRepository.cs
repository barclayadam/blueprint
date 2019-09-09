using System;
using System.Threading.Tasks;

namespace Blueprint.Api
{
    public interface IResourceEventRepository
    {
        Task<object> GetCurrentDataAsync(string href, Type resourceType);

        Task AddAsync(ResourceEvent resourceEvent);
    }
}