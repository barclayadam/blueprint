using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Blueprint.Core.Validation
{
    /// <summary>
    /// The property this attribute is attached to must be <c>true</c>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "errorMessageAccessor: This belongs to the base class and cannot have a public accessor.")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class MustBeTrueAttribute : ValidationAttribute
    {
        /// <summary>
        /// Validates that the provided value is true.
        /// </summary>
        /// <param name="value">The boolean to evaluate.</param>
        /// <returns>
        /// <c>true</c> if the value is <c>null</c> or a value representing <c>true</c>, <c>false</c> otherwise.
        /// </returns>
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            bool boolValue;
            
            return bool.TryParse(value.ToString(), out boolValue) && boolValue;
        }
    }
}