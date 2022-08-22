using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint.Authorisation;

namespace Blueprint;

/// <summary>
/// An implementation of <see cref="IUserAuthorisationContextFactory" /> that will always return
/// <see cref="AnonymousUserAuthorisationContext.Instance" />.
/// </summary>
public class AnonymousUserAuthorisationContextFactory : IUserAuthorisationContextFactory
{
    /// <inheritdoc />
    public Task<IUserAuthorisationContext> CreateContextAsync(ClaimsIdentity claimsIdentity)
    {
        return Task.FromResult((IUserAuthorisationContext)AnonymousUserAuthorisationContext.Instance);
    }
}