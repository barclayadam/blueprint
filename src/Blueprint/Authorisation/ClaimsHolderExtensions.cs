using System.Collections.Generic;
using System.Security.Claims;

namespace Blueprint.Authorisation
{
    public static class ClaimsHolderExtensions
    {
        public static IClaimsHolder ToClaimsHolder(this IEnumerable<Claim> claims)
        {
            return new ClaimsHolder(claims);
        }
    }
}
