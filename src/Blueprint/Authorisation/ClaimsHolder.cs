using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Blueprint.Authorisation
{
    public class ClaimsHolder : IClaimsHolder
    {
        private readonly ILookup<string, Claim> claimsByValueType;

        public ClaimsHolder(IEnumerable<Claim> claims)
        {
            Guard.NotNull(nameof(claims), claims);

            Claims = claims;

            claimsByValueType = claims.ToLookup(c => c.ValueType);
        }

        public IEnumerable<Claim> Claims { get; }

        public IEnumerable<Claim> GetClaimsByValueType(string valueType)
        {
            return claimsByValueType[valueType];
        }

        public bool ContainsValueType(string name)
        {
            return claimsByValueType.Contains(name);
        }
    }
}
