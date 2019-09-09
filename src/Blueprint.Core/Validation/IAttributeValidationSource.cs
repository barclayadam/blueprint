namespace Blueprint.Core.Validation
{
    using System.Reflection;
    using System.Threading.Tasks;

    using Api;

    public interface IAttributeValidationSource : IValidationSource
    {
        Task AddAttributeValidationResultsAsync(PropertyInfo propertyInfo, object value, ApiOperationContext apiOperationContext, ValidationFailures results);
    }
}