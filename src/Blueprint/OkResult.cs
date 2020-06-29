using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint
{
    /// <summary>
    /// Represents a known result, a well-handled result of executing an operation that will use conneg to
    /// determine _how_ to output a value and also set the status code of the HTTP response.
    /// </summary>
    public class OkResult : OperationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OkResult" /> class with the given content.
        /// </summary>
        /// <param name="content">The content to write to the body of the output.</param>
        public OkResult(object content)
        {
            Content = content;
        }

        /// <summary>
        /// Gets or sets the content that will be output when this result is executed.
        /// </summary>
        public object Content { get; set; }

        /// <inheritdoc />
        public override Task ExecuteAsync(ApiOperationContext context)
        {
            var executor = context.ServiceProvider.GetRequiredService<IOperationResultExecutor<OkResult>>();

            return executor.ExecuteAsync(context, this);
        }
    }

    /// <summary>
    /// A specialisation of <see cref="OkResult" /> that has the content it expects typed to a
    /// known type.
    /// </summary>
    /// <typeparam name="T">The type of content that will be used as the body of this result.</typeparam>
    public class OkResult<T> : OkResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OkResult" /> class with the given content.
        /// </summary>
        /// <param name="content">The content to write to the body of the output.</param>
        public OkResult(T content) : base(content)
        {
        }

        /// <summary>
        /// Gets or sets the content that will be output when this result is executed.
        /// </summary>
        public new T Content => (T)base.Content;
    }
}
