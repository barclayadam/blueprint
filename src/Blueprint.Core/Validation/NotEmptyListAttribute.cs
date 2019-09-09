using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Blueprint.Core.Properties;

namespace Blueprint.Core.Validation
{
    /// <summary>
    /// Requires the property this attribute is applied to, to have at least one item in its collection.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments",
            Justification = "errorMessageAccessor belongs to the base class, thus cannot have a public accessor.")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property,
            AllowMultiple = false)]
    public sealed class NotEmptyListAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotEmptyListAttribute"/> class.
        /// </summary>
        public NotEmptyListAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotEmptyListAttribute"/> class.
        /// </summary>
        /// <param name="errorMessage">
        /// The error message.
        /// </param>
        public NotEmptyListAttribute(string errorMessage)
                : base(errorMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotEmptyListAttribute"/> class.
        /// </summary>
        /// <param name="errorMessageAccessor">
        /// The function that enables access to validation resources.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="errorMessageAccessor"/> is null.
        /// </exception>
        public NotEmptyListAttribute(Func<string> errorMessageAccessor)
                : base(errorMessageAccessor)
        {
        }

        /// <summary>
        /// Evaluates if the provided collection is a filled list. A null object also returns as valid.
        /// </summary>
        /// <param name="value">
        /// The value to be checked.
        /// </param>
        /// <returns>
        /// Whether or not the value is an enumerable value that has at least one item.
        /// </returns>
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is IEnumerable)
            {
                return ((IEnumerable<object>)value).Any();
            }

            throw new InvalidOperationException(Resources.NotEmptyListAttribute_IsValid_NonEnumerableException_Message);
        }
    }
}
