using System.Security.Claims;

namespace Blueprint.Authorisation;

public interface IClaimsIdentityProvider
{
    ClaimsIdentity Get(ApiOperationContext context);
}