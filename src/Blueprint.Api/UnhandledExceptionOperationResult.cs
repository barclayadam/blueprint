using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Blueprint.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api
{
    /// <summary>
    /// An <see cref="OperationResult" /> that represents an unhandled exception that is caught by the pipeline
    /// executor, delegating it's execution to a registered <see cref="IOperationResultExecutor{TResult}" />.
    /// </summary>
    public class UnhandledExceptionOperationResult : OperationResult
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="UnhandledExceptionOperationResult" /> class with the
        /// given <see cref="Exception" />.
        /// </summary>
        /// <param name="exception">The unhandled exception.</param>
        public UnhandledExceptionOperationResult(Exception exception)
        {
            Guard.NotNull(nameof(exception), exception);

            Exception = exception;
        }

        /// <summary>
        /// Gets the <see cref="Exception" /> this result represents.
        /// </summary>
        public Exception Exception { get; }

        /// <inheritdoc />
        public override Task ExecuteAsync(ApiOperationContext context)
        {
            var executor = context.ServiceProvider.GetService<IOperationResultExecutor<UnhandledExceptionOperationResult>>();

            return executor.ExecuteAsync(context, this);
        }

        /// <summary>
        /// Rethrows the <see cref="Exception" /> that this <see cref="UnhandledExceptionOperationResult" />
        /// represents.
        /// </summary>
        public void Rethrow()
        {
            ExceptionDispatchInfo.Capture(Exception).Throw();
        }
    }
}
