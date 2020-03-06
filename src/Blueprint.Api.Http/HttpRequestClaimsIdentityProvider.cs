using System.Linq;
using System.Security.Claims;
using Blueprint.Api.Authorisation;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Api.Http
{
    /// <summary>
    /// An <see cref="IClaimsIdentityProvider" /> that will use the <see cref="HttpContext" /> registered
    /// with the <see cref="ApiOperationContext"/>.
    /// </summary>
    public class HttpRequestClaimsIdentityProvider : IClaimsIdentityProvider
    {
        /// <summary>
        /// Gets the <see cref="ClaimsIdentity" /> from the <see cref="HttpContext" /> of the operation
        /// context.
        /// </summary>
        /// <param name="context">The context to grab the identity from.</param>
        /// <returns>The (first) <see cref="ClaimsIdentity" /> from the <see cref="HttpContext"/>.</returns>
        public ClaimsIdentity Get(ApiOperationContext context)
        {
            return context.GetHttpContext().Request.HttpContext.User.Identities.First();
        }
    }
}
