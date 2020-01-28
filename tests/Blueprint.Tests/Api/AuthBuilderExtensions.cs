using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Testing;

namespace Blueprint.Tests.Api
{
    public static class AuthBuilderExtensions
    {
        public static Task<OperationResult> ExecuteWithAuth(this TestApiOperationExecutor executor, IApiOperation operation, params Claim[] claims)
        {
            var context = executor.ContextFor(operation);

            context.ClaimsIdentity = new ClaimsIdentity(claims, "TestAuth");

            return executor.ExecuteAsync(context);
        }
    }
}