namespace Blueprint.Api.Errors
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message)
            : base("The requested resource could not be found", "not_found", message, 404)
        {
        }
    }
}
