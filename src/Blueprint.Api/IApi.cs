using System.Threading.Tasks;

namespace Blueprint.Api
{
    public interface IApi
    {
        Task<OperationResult> ExecuteAsync(IApiOperation operation);
    }
}
