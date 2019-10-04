using System.Security.Claims;

namespace Blueprint.Api.Authorisation
{
    public interface IClaimsIdentityProvider
    {
        ClaimsIdentity Get(ApiOperationContext context);
    }
}
