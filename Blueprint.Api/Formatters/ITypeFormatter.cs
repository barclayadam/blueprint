namespace Blueprint.Api.Formatters
{
    public interface ITypeFormatter
    {
        bool IsSupported(ApiOperationContext context, string format);

        void Write(ApiOperationContext context, string format, object result);
    }
}