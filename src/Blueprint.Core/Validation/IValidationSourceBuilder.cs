using System.Collections.Generic;

using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Core.Validation
{
    /// <summary>
    /// Represents a builder that can perform code generation at a property and class level to build efficient
    /// validation of operations.
    /// </summary>
    interface IValidationSourceBuilder
    {
        /// <summary>
        /// Gets frames that should be added to perform validation of a property represented by the given
        /// parameters.
        /// </summary>
        /// <remarks>
        /// Typically this method will return 0 or 1 <see cref="Frame"/> instances, 0 in the case that the property does
        /// not actually have any validatable attributes, or 1 to perform the validation.
        /// </remarks>
        /// <param name="operationVariable">The variable that represents the overall operation being validated.</param>
        /// <param name="properties">All properties of the operation, with variables that can be used to access the property and it's value at runtime.</param>
        /// <returns>0 or more frames used to validate an operation.</returns>
        IEnumerable<Frame> GetFrames(Variable operationVariable, List<OperationProperty> properties);
    }
}
