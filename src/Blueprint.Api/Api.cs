using System.Threading.Tasks;

namespace Blueprint.Api
{
    public class Api : IApi
    {
        private readonly CodeGennedExecutor codeGennedExecutor;

        public Api(CodeGennedExecutor codeGennedExecutor)
        {
            this.codeGennedExecutor = codeGennedExecutor;
        }

        public Task<OperationResult> ExecuteAsync(IApiOperation operation)
        {
            return codeGennedExecutor.ExecuteAsync(new ApiOperationContext(null, null, null));
        }
    }
}
