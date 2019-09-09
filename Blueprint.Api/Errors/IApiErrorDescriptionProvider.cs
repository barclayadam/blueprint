namespace Blueprint.Api.Errors
{
    public interface IApiErrorDescriptionProvider
    {
        string ErrorCode { get; }

        string ErrorMessage { get; }
    }
}
