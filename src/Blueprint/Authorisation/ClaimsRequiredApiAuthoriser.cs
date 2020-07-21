using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blueprint.Authorisation
{
    public class ClaimsRequiredApiAuthoriser : IApiAuthoriser
    {
        private static readonly Task<ExecutionAllowed> UserUnauthenticated = Task.FromResult(ExecutionAllowed.No("Operation does not have AllowAnonymous attribute. User is unauthenticated", "Please log in", ExecutionAllowedFailureType.Authentication));
        private static readonly Task<ExecutionAllowed> UserInactive = Task.FromResult(ExecutionAllowed.No("Operation does not have AllowAnonymous attribute. User is inactive", "Your account has been deactivated", ExecutionAllowedFailureType.Authentication));

        private readonly IClaimInspector claimInspector;

        public ClaimsRequiredApiAuthoriser(IClaimInspector claimInspector)
        {
            Guard.NotNull(nameof(claimInspector), claimInspector);

            this.claimInspector = claimInspector;
        }

        public bool AppliesTo(ApiOperationDescriptor descriptor)
        {
            return descriptor.TypeAttributes.OfType<ClaimRequiredAttribute>().SingleOrDefault() != null;
        }

        public Task<ExecutionAllowed> CanExecuteOperationAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object operation)
        {
            return IsAuthorisedAsync(operationContext, descriptor, operation);
        }

        public Task<ExecutionAllowed> CanShowLinkAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object resource)
        {
            return IsAuthorisedAsync(operationContext, descriptor, resource);
        }

        private Task<ExecutionAllowed> IsAuthorisedAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object resource)
        {
            if (operationContext.UserAuthorisationContext == null)
            {
                return UserUnauthenticated;
            }

            if (!operationContext.UserAuthorisationContext.IsActive)
            {
                return UserInactive;
            }

            if (!(operationContext.UserAuthorisationContext is IClaimsHolder userClaims))
            {
                throw new InvalidOperationException($"Cannot apply {nameof(ClaimsRequiredApiAuthoriser)} to a user context that does not implement {nameof(IClaimsHolder)}");
            }

            var claimRequiredAttribute = descriptor.TypeAttributes.OfType<ClaimRequiredAttribute>().Single();

            var expansionState = ClaimExpansionState.RequiresExpansion;
            var requiredClaim = claimRequiredAttribute.GetClaim(resource);

            // Pre-expand key if it is a match
            var apiResource = resource as IHaveResourceKey;
            var expandedKey = apiResource?.ResourceKey;

            if (expandedKey != null && expandedKey.EndsWith(requiredClaim.Value))
            {
                requiredClaim = new Claim(requiredClaim.Type, expandedKey, requiredClaim.ValueType);
                expansionState = ClaimExpansionState.AlreadyExpanded;
            }

            var result = claimInspector.IsDemandedClaimFulfilled(userClaims, requiredClaim, expansionState);

            if (!result)
            {
                return Task.FromResult(ExecutionAllowed.No(
                    $"User does not have required claim {requiredClaim.Type} {requiredClaim.ValueType} for {requiredClaim.Value}",
                    "You do not have enough permissions to perform this action",
                    ExecutionAllowedFailureType.Authorisation));
            }

            return ExecutionAllowed.YesTask;
        }
    }
}
