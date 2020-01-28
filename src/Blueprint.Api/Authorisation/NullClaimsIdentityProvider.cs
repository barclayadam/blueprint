using System.Security.Claims;

namespace Blueprint.Api.Authorisation
{
    /// <summary>
    /// Implements <see cref="IClaimsIdentityProvider" /> to always return <c>null</c>, used by default
    /// when we assume that the <see cref="ApiOperationContext.ClaimsIdentity" /> property will be set from
    /// <c>outside</c> the pipeline execution and therefore need no other source of identity.
    /// </summary>
    public class NullClaimsIdentityProvider : IClaimsIdentityProvider
    {
        /// <summary>
        /// Returns <c>null</c>.
        /// </summary>
        /// <param name="context">The operation context, ignored.</param>
        /// <returns><c>null</c>.</returns>
        public ClaimsIdentity Get(ApiOperationContext context)
        {
            return null;
        }
    }
}
