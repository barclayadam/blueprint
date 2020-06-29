using System.Security.Claims;

namespace Blueprint.Authorisation
{
    public interface IClaimInspector
    {
        /// <summary>
        /// Checks to see whether any claims the given <see cref="ClaimsPrincipal" /> has would satisfy the
        /// given demanded claim.
        /// </summary>
        /// <param name="userClaims">The claims the user wishes to access a resource has.</param>
        /// <param name="demandedClaim">The claim that has been demanded and should be checked for.</param>
        /// <param name="claimExpansionState">The state of expansion, which can be used to indicate expansion has
        /// already happened and should not happen again.</param>
        /// <returns>Whether the given claim has been "fulfilled" by the list of claims of the user.</returns>
        bool IsDemandedClaimFulfilled(IClaimsHolder userClaims, Claim demandedClaim, ClaimExpansionState claimExpansionState);
    }
}
