using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Middleware
{
    /// <summary>
    /// An <see cref="OperationResult" /> that can be returned in the case of problems in validation
    /// of an operation.
    /// </summary>
    public class ValidationFailedOperationResult : OperationResult
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ValidationFailedOperationResult" /> class with a single,
        /// form-level validation message (used when the overall message fails validation as opposed to a specific
        /// property).
        /// </summary>
        /// <param name="formLevelErrorMessage">The form-level validation message.</param>
        public ValidationFailedOperationResult(string formLevelErrorMessage)
            : this(new Dictionary<string, IEnumerable<string>>
            {
                [ValidationFailures.FormLevelPropertyName] = new[] { formLevelErrorMessage },
            })
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ValidationFailedOperationResult" /> with a dictionary
        /// of property &lt;-&gt; property errors messages.
        /// </summary>
        /// <param name="errors">The errors of this failure result.</param>
        public ValidationFailedOperationResult(Dictionary<string, IEnumerable<string>> errors)
        {
            Errors = errors;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ValidationFailedOperationResult" /> with the errors dictionary
        /// as specified in the supplied <see cref="ValidationFailures" />.
        /// </summary>
        /// <param name="errors">The validation errors.</param>
        public ValidationFailedOperationResult(ValidationFailures errors)
        {
            Errors = errors.AsDictionary();
        }

        /// <summary>
        /// A dictionary of errors describing the validation failures.
        /// </summary>
        public Dictionary<string, IEnumerable<string>> Errors { get; private set; }

        /// <inheritdoc />
        public override Task ExecuteAsync(ApiOperationContext context)
        {
            var executor = context.ServiceProvider.GetRequiredService<IOperationResultExecutor<ValidationFailedOperationResult>>();

            return executor.ExecuteAsync(context, this);
        }
    }
}
