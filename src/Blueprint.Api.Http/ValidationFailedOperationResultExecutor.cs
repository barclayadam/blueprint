using System.Threading.Tasks;
using Blueprint.Api.Middleware;

namespace Blueprint.Api.Http
{
    /// <summary>
    /// An <see cref="IOperationResultExecutor{TResult}" /> for <see cref="UnhandledExceptionOperationResult" /> that will
    /// interrogate the <see cref="UnhandledExceptionOperationResult.Exception" /> to convert to the most appropriate
    /// HTTP response
    /// </summary>
    public class ValidationFailedOperationResultExecutor : IOperationResultExecutor<ValidationFailedOperationResult>
    {
        private readonly OkResultOperationExecutor okResultOperationExecutor;

        /// <summary>
        /// Initialises a new instance of the <see cref="UnhandledExceptionOperationResultExecutor" /> class.
        /// </summary>
        /// <param name="okResultOperationExecutor">The <see cref="OkResultOperationExecutor"/> writing is delegated to.</param>
        public ValidationFailedOperationResultExecutor(OkResultOperationExecutor okResultOperationExecutor)
        {
            this.okResultOperationExecutor = okResultOperationExecutor;
        }

        /// <inheritdoc />
        public Task ExecuteAsync(ApiOperationContext context, ValidationFailedOperationResult result)
        {
            var validationProblemDetails = new ValidationProblemDetails(result.Errors);

            return okResultOperationExecutor.WriteContentAsync(
                context.GetHttpContext(),
                validationProblemDetails.Status.Value,
                validationProblemDetails);
        }
    }
}
