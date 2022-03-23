using Blueprint.Authorisation;
using Blueprint.Errors;

namespace Blueprint.Sample.WebApi.Api
{
    [RootLink("private/command")]
    [ClaimRequired("role", "/customer/1", nameof(AuthorisationRequiredCommand))]
    public class AuthorisationRequiredCommand : ICommand
    {
        public void Invoke()
        {
            throw new UnauthorizedException("Unable to process, you are not authorised even though you passed basic tests");
        }
    }
}
