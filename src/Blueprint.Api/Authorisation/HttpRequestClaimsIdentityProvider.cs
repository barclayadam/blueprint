using System.Linq;
using System.Security.Claims;

namespace Blueprint.Api.Authorisation
{
    public class HttpRequestClaimsIdentityProvider : IClaimsIdentityProvider
    {
        public ClaimsIdentity Get(ApiOperationContext context)
        {
            return context.Request.HttpContext.User.Identities.First();
        }
    }
}
