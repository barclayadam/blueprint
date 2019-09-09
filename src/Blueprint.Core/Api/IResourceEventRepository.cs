namespace Blueprint.Core.Api
{
    using System;
    using System.Threading.Tasks;

    public interface IResourceEventRepository
    {
        Task<object> GetCurrentDataAsync(string href, Type resourceType);

        Task AddAsync(ResourceEvent resourceEvent);
    }
}