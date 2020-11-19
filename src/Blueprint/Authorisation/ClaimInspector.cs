using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Blueprint.Caching;
using Microsoft.Extensions.Logging;

namespace Blueprint.Authorisation
{
    public class ClaimInspector : IClaimInspector
    {
        private readonly IEnumerable<IResourceKeyExpander> _resourceKeyExpanders;
        private readonly ICache _cache;
        private readonly ILogger<ClaimInspector> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimInspector"/> class.
        /// </summary>
        /// <param name="resourceKeyExpanders">Resource key expanders that will be asked to 'expand' any demanded claims.</param>
        /// <param name="cache">Cache used to store expanded resource keys (as they will typically require database access).</param>
        /// <param name="logger">Logger to use.</param>
        public ClaimInspector(IEnumerable<IResourceKeyExpander> resourceKeyExpanders, ICache cache, ILogger<ClaimInspector> logger)
        {
            Guard.NotNull(nameof(resourceKeyExpanders), resourceKeyExpanders);
            Guard.NotNull(nameof(cache), cache);

            this._resourceKeyExpanders = resourceKeyExpanders;
            this._cache = cache;
            this._logger = logger;
        }

        /// <summary>
        /// Checks to see whether any claims the given <see cref="ClaimsPrincipal" /> has would satisfy the
        /// given demanded claim.
        /// </summary>
        /// <param name="userClaims">The claims the user wishes to access a resource has.</param>
        /// <param name="demandedClaim">The claim that has been demanded and should be checked for.</param>
        /// <param name="claimExpansionState">The state of expansion, which can be used to indicate expansion has
        /// already happened and should not happen again.</param>
        /// <returns>Whether the demanded claim can be fulfilled by the list of claims of the user.</returns>
        public bool IsDemandedClaimFulfilled(IClaimsHolder userClaims, Claim demandedClaim, ClaimExpansionState claimExpansionState)
        {
            if (demandedClaim == null)
            {
                this._logger.LogTrace("No claim demanded, returning ClaimInspectionResult.Success");

                return true;
            }

            if (claimExpansionState == ClaimExpansionState.AlreadyExpanded)
            {
                return this.IsExpandedClaimFulfilled(userClaims, demandedClaim);
            }

            var expandedClaim = demandedClaim;

            if (demandedClaim.Value == "*")
            {
                this._logger.LogTrace("Wildcard claim given, no expansion necessary.");
            }
            else if (!this._resourceKeyExpanders.Any())
            {
                this._logger.LogTrace("No resource key expanders");
            }
            else
            {
                expandedClaim = this.ExpandClaim(demandedClaim);
            }

            return this.IsExpandedClaimFulfilled(userClaims, expandedClaim);
        }

        private bool IsExpandedClaimFulfilled(IClaimsHolder issuedClaims, Claim demandedClaim)
        {
            Guard.NotNull(nameof(issuedClaims), issuedClaims);

            // This is a foreach loop, not LINQ, because it is called so many times. We do not want Lambda overhead, deferred
            // execution etc.
            foreach (var claim in issuedClaims.GetClaimsByValueType(demandedClaim.ValueType))
            {
                if (claim.Type != demandedClaim.Type)
                {
                    continue;
                }

                if (demandedClaim.Value == "*")
                {
                    this._logger.LogTrace("Wildcard claim given, ValueType and Type match found, returning true.");
                    return true;
                }

                if (demandedClaim.Value.Contains(claim.Value))
                {
                    this._logger.LogTrace("Expanded claim was fulfilled, returning ClaimInspectionResult.Success.");
                    return true;
                }
            }

            if (this._logger.IsEnabled(LogLevel.Trace))
            {
                this._logger.LogTrace("Expanded claim '[{0}, {1}, {2}]' was not fulfilled.", demandedClaim.Type, demandedClaim.Value, demandedClaim.ValueType);
            }

            return false;
        }

        private Claim ExpandClaim(Claim claim)
        {
            this._logger.LogTrace("Expanding required claim {0}.", claim);

            var expandedKey = this._cache.GetOrCreate(
                "Resource Key Expansion",
                claim.Value,
                () =>
                {
                    return this._resourceKeyExpanders.Select(k => k.Expand(claim.Value)).FirstOrDefault(x => x != null);
                });

            if (expandedKey == null)
            {
                return claim;
            }

            return new Claim(claim.Type, expandedKey, claim.ValueType);
        }
    }
}
