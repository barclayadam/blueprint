using System.Security;
using Blueprint.Authorisation;

namespace Blueprint.Sample.WebApi.Api
{
    [RootLink("private/command")]
    [ClaimRequired("role", "/customer/1", nameof(AuthorisationRequiredCommand))]
    public class AuthorisationRequiredCommand : ICommand
    {
        public void Invoke()
        {
            throw new SecurityException("Unable to process, you are not authorised even though you passed basic tests");
        }
    }
}
