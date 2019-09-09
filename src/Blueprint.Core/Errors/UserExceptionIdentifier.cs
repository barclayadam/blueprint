using Blueprint.Core.Security;

namespace Blueprint.Core.Errors
{
    public class UserExceptionIdentifier
    {
        public UserExceptionIdentifier(IUserAuthorisationContext context)
        {
            if (context == null)
            {
                IsAnonymous = true;
            }
            else
            {
                IsAnonymous = context.IsAnonymous;
                Id = context.Id;
                Email = context.Email;
                Name = context.Name;
            }
        }

        public UserExceptionIdentifier(string id)
        {
            IsAnonymous = false;
            Id = id;
        }

        public string Id { get; }

        public bool IsAnonymous { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }
    }
}
