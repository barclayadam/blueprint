using System.Threading.Tasks;

namespace Blueprint.Validation
{
    /// <summary>
    /// A validator that can, given an object, validate its current state to ensure that it
    /// is considered 'valid', whatever that may represent in the context of using a validator.
    /// </summary>
    /// <remarks>
    /// Typically DataAnnotations will be used to specify the validation rules for an object (e.g.
    /// an operation), using the concrete of implementation of this interface of <see cref="BlueprintValidator"/>.
    /// </remarks>
    public interface IValidator
    {
        /// <summary>
        /// Given an object will get the validation failures based on its current state, with
        /// implementers of this method determing how the validation rules are to be specified.
        /// </summary>
        /// <param name="value">The value to be checked.</param>
        /// <param name="apiOperationContext">The current API context.</param>
        /// <returns>The validation results.</returns>
        Task<ValidationFailures> GetValidationResultsAsync(object value, ApiOperationContext apiOperationContext);
    }
}
