using System.Security.Claims;
using System.Threading.Tasks;

namespace Blueprint.Core.Authorisation
{
    public interface IUserAuthorisationContextFactory
    {
        Task<IUserAuthorisationContext> CreateContextAsync(ClaimsIdentity claimsIdentity);
    }
}
