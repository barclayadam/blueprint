using System.Threading.Tasks;

namespace Blueprint.Api.Formatters
{
    public interface ITypeFormatter
    {
        bool IsSupported(ApiOperationContext context, string format);

        Task WriteAsync(ApiOperationContext context, string format, object result);
    }
}
