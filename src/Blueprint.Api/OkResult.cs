using System.Net;
using System.Threading.Tasks;
using Blueprint.Api.Formatters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api
{
    /// <summary>
    /// Represents a known result, a well-handled result of executing an operation that will use conneg to
    /// determine _how_ to output a value and also set the status code of the HTTP response.
    /// </summary>
    public class OkResult : OperationResult
    {
        private readonly object content;
        private readonly HttpStatusCode? statusCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="OkResult" /> class with the given content
        /// and status code of <see cref="HttpStatusCode.OK" />.
        /// </summary>
        /// <param name="content">The content to write to the body of the output.</param>
        public OkResult(object content) : this(HttpStatusCode.OK, content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OkResult" /> class with the given content
        /// and status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to set.</param>
        /// <param name="content">The content to write to the body of the output.</param>
        public OkResult(HttpStatusCode statusCode, object content)
        {
            this.statusCode = statusCode;
            this.content = content;
        }

        /// <summary>
        /// Gets the content that will be output when this result is executed.
        /// </summary>
        public object Content => content;

        public override async Task ExecuteAsync(ApiOperationContext context)
        {
            if (statusCode != null)
            {
                context.Response.StatusCode = (int)statusCode;
            }

            var requestedFormat = GetRequestedFormat(context.Request);
            var formatter = requestedFormat != null ? GetFormatter(context, requestedFormat) : context.ServiceProvider.GetRequiredService<JsonTypeFormatter>();

            await formatter.WriteAsync(context, requestedFormat, content);
        }

        private static string GetRequestedFormat(HttpRequest request)
        {
            // Bail early if no query string to avoid allocations on getting the query string
            // values (GetQueryNameValuePairs)
            if (request == null || request.QueryString.HasValue == false)
            {
                return null;
            }

            if (request.Query.TryGetValue("format", out var format))
            {
                return format[0];
            }

            return null;
        }

        private ITypeFormatter GetFormatter(ApiOperationContext context, string requestedFormat)
        {
            var typeFormatters = context.ServiceProvider.GetServices<ITypeFormatter>();

            // PERF: Do not use LINQ to avoid allocating closure
            foreach (var formatter in typeFormatters)
            {
                if (formatter.IsSupported(context, requestedFormat))
                {
                    return formatter;
                }
            }

            throw new ApiException(
                $"{requestedFormat} is not supported",
                "unsupported_format",
                HttpStatusCode.UnsupportedMediaType);
        }
    }

    /// <summary>
    /// A specialisation of <see cref="OkResult" /> that has the content it expects typed to a
    /// known type.
    /// </summary>
    /// <typeparam name="T">The type of content that will be used as the body of this result.</typeparam>
    public class OkResult<T> : OkResult
    {
        public OkResult(T content) : base(content)
        {
        }

        public OkResult(HttpStatusCode statusCode, T content) : base(statusCode, content)
        {
        }

        public new T Content => (T)base.Content;
    }
}
