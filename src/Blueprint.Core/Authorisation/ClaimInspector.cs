using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using Blueprint.Core.Caching;

using NLog;

namespace Blueprint.Core.Authorisation
{
    public class ClaimInspector : IClaimInspector
    {
        private static readonly Logger Log = LogManager.GetLogger("Authorisation");

        private readonly IEnumerable<IResourceKeyExpander> resourceKeyExpanders;
        private readonly ICache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimInspector"/> class.
        /// </summary>
        /// <param name="resourceKeyExpanders">Resource key expanders that will be asked to 'expand' any demanded claims.</param>
        /// <param name="cache">Cache used to store expanded resource keys (as they will typically require database access).</param>
        public ClaimInspector(IEnumerable<IResourceKeyExpander> resourceKeyExpanders, ICache cache)
        {
            Guard.NotNull(nameof(resourceKeyExpanders), resourceKeyExpanders);
            Guard.NotNull(nameof(cache), cache);

            this.resourceKeyExpanders = resourceKeyExpanders;
            this.cache = cache;
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
                Log.Trace("No claim demanded, returning ClaimInspectionResult.Success");

                return true;
            }

            if (claimExpansionState == ClaimExpansionState.AlreadyExpanded)
            {
                return IsExpandedClaimFulfilled(userClaims, demandedClaim);
            }

            var expandedClaim = demandedClaim;

            if (demandedClaim.Value == "*")
            {
                Log.Trace("Wildcard claim given, no expansion necessary.");
            }
            else if (!resourceKeyExpanders.Any())
            {
                Log.Trace("No resource key expanders");
            }
            else
            {
                expandedClaim = ExpandClaim(demandedClaim);
            }

            return IsExpandedClaimFulfilled(userClaims, expandedClaim);
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
                    Log.Trace("Wildcard claim given, ValueType and Type match found, returning true.");
                    return true;
                }

                if (demandedClaim.Value.Contains(claim.Value))
                {
                    Log.Trace("Expanded claim was fulfilled, returning ClaimInspectionResult.Success.");
                    return true;
                }
            }

            if (Log.IsTraceEnabled)
            {
                Log.Trace("Expanded claim '[{0}, {1}, {2}]' was not fulfilled.", demandedClaim.Type, demandedClaim.Value, demandedClaim.ValueType);
            }

            return false;
        }

        private Claim ExpandClaim(Claim claim)
        {
            Log.Trace("Expanding required claim {0}.", claim);

            var expandedKey = cache.GetOrCreate(
                "Resource Key Expansion",
                claim.Value,
                () =>
                {
                    return resourceKeyExpanders.Select(k => k.Expand(claim.Value)).FirstOrDefault(x => x != null);
                });

            if (expandedKey == null)
            {
                return claim;
            }

            return new Claim(claim.Type, expandedKey, claim.ValueType);
        }
    }
}
