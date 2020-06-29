using System.Collections.Generic;
using System.Security.Claims;

namespace Blueprint.Authorisation
{
    public interface IClaimsHolder
    {
        IEnumerable<Claim> GetClaimsByValueType(string valueType);
    }
}
