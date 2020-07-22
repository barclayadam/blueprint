using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint.Authorisation;

namespace Blueprint.Tests
{
    public class TestUserAuthorisationContextFactory : IUserAuthorisationContextFactory
    {
        public Task<IUserAuthorisationContext> CreateContextAsync(ClaimsIdentity claimsIdentity)
        {
            return Task.FromResult((IUserAuthorisationContext) new TestUserAuthorisationContext
            {
                Claims = claimsIdentity.Claims.ToList()
            });
        }
    }
}
