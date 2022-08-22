using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Blueprint.Authorisation;

public class ClaimsHolder : IClaimsHolder
{
    private readonly ILookup<string, Claim> _claimsByValueType;

    public ClaimsHolder(IEnumerable<Claim> claims)
    {
        Guard.NotNull(nameof(claims), claims);

        this.Claims = claims;

        this._claimsByValueType = claims.ToLookup(c => c.ValueType);
    }

    public IEnumerable<Claim> Claims { get; }

    public IEnumerable<Claim> GetClaimsByValueType(string valueType)
    {
        return this._claimsByValueType[valueType];
    }

    public bool ContainsValueType(string name)
    {
        return this._claimsByValueType.Contains(name);
    }
}