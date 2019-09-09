using System.Collections.Generic;
using System.Reflection;

namespace Blueprint.Core.Api
{
    /// <summary>
    /// Provides conversion from server-side validation attributes for a property to a client-side
    /// representation to provide the same validation at a client level to improve experience and
    /// avoid having to go all the way through to the server to detect simple validation
    /// problems.
    /// </summary>
    public interface IPropertyValidatorProvider
    {
        /// <summary>
        /// Given a property converts the validation attributes defined on that property
        /// into the necessary metadata for the client-side validation framework to implement
        /// the equivalent validation checks.
        /// </summary>
        /// <param name="propertyInfo">The property to convert.</param>
        /// <returns>The, potentially empty, list of client-side property validators for the
        /// given property.</returns>
        IEnumerable<PropertyValidator> GetClientSideValidators(PropertyInfo propertyInfo);
    }
}