namespace Blueprint.Core.Errors
{
    public interface IApiErrorDescriptionProvider
    {
        string ErrorCode { get; }

        string ErrorMessage { get; }
    }
}