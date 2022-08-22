using System;
using System.Threading.Tasks;

namespace Blueprint.Http;

public interface IResourceEventRepository
{
    Task<object> GetCurrentDataAsync(string href, Type resourceType);

    Task AddAsync(ResourceEvent resourceEvent);
}