using System.Collections.Generic;
using System.Security.Claims;

namespace Blueprint.Core.Authorisation
{
    public interface IClaimsHolder
    {
        IEnumerable<Claim> GetClaimsByValueType(string valueType);
    }
}