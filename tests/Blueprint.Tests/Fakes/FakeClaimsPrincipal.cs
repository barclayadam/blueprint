using System.Security.Claims;
using System.Security.Principal;

namespace Blueprint.Tests.Fakes;

public class FakeClaimsPrincipal : ClaimsPrincipal
{
    public FakeClaimsPrincipal(string username, params Claim[] claims)
        : base(new GenericIdentity(username))
    {
        ((GenericIdentity) Identity).AddClaims(claims);
    }
}