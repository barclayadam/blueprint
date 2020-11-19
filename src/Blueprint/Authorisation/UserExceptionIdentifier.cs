namespace Blueprint.Authorisation
{
    public class UserExceptionIdentifier
    {
        public UserExceptionIdentifier(IUserAuthorisationContext context)
        {
            if (context == null)
            {
                this.IsAnonymous = true;
            }
            else
            {
                this.IsAnonymous = context.IsAnonymous;
                this.Id = context.Id;
                this.Email = context.Email;
                this.Name = context.Name;
            }
        }

        public UserExceptionIdentifier(string id)
        {
            this.IsAnonymous = false;
            this.Id = id;
        }

        public string Id { get; }

        public bool IsAnonymous { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }
    }
}
