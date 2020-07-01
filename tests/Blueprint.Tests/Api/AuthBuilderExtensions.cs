using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint.Testing;

namespace Blueprint.Tests.Api
{
    public static class AuthBuilderExtensions
    {
        public static Task<OperationResult> ExecuteWithAuth(this TestApiOperationExecutor executor, IApiOperation operation, params Claim[] claims)
        {
            var context = executor.ContextFor(operation);

            context.WithAuth(claims);

            return executor.ExecuteAsync(context);
        }

        public static ApiOperationContext WithAuth(this ApiOperationContext context, params Claim[] claims)
        {
            context.ClaimsIdentity = new ClaimsIdentity(claims, "TestAuth");

            return context;
        }
    }
}
