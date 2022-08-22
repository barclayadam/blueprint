namespace Blueprint.Errors;

public class BadRequestException : ApiException
{
    public BadRequestException(string title, string type, string detail)
        : base(title, type, detail, 400)
    {
    }
}