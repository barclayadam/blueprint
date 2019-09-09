using System.Collections.Generic;

namespace Blueprint.Core.Errors
{
    /// <summary>
    /// A provider of contextual error data that will be used whenever logging exceptions/errors
    /// to push to providers for easier diagnosis (for example HTTP data, current user data).
    /// </summary>
    public interface IErrorDataProvider
    {
        /// <summary>
        /// Populates the given <see cref="Dictionary{TKey,TValue}" /> with any values that would
        /// assist debugging the exception being recorded.
        /// </summary>
        /// <param name="errorData">The dictionary to populate.</param>
        void Populate(Dictionary<string, string> errorData);
    }
}
