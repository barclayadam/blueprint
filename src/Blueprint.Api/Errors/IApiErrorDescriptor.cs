namespace Blueprint.Api.Errors
{
    public interface IApiErrorDescriptor
    {
        string ErrorCode { get; }

        string ErrorMessage { get; }
    }
}
