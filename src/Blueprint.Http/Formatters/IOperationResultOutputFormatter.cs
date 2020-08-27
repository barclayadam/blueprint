using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Http.Formatters
{
    /// <summary>
    /// Provides formatting of results, such that a result can be converted to a text-representation
    /// such as JSON or XML, or potentially binary files like PDFs.
    /// </summary>
    public interface IOperationResultOutputFormatter
    {
        /// <summary>
        /// Indicates whether this formatter supports the given request, typically by inspecting
        /// the Accept / Content-Type headers sent as part of the request.
        /// </summary>
        /// <param name="context">The context used to determine whether this formatter is supported.</param>
        /// <returns>Whether this formatter is supported for the given request and format.</returns>
        bool IsSupported(OutputFormatterCanWriteContext context);

        /// <summary>
        /// Writes the given result (as produced by a Blueprint API operation pipeline) to the
        /// <seealso cref="HttpResponse" />.
        /// </summary>
        /// <param name="context">The write context, the same that would have been passed to <see cref="IsSupported" />.</param>
        /// <returns>A <see cref="Task" /> representing the async writing of this formatter.</returns>
        Task WriteAsync(OutputFormatterCanWriteContext context);
    }
}
