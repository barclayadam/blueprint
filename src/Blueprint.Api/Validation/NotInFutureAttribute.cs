using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Blueprint.Core;
using Blueprint.Core.Properties;
using Blueprint.Core.Utilities;

namespace Blueprint.Api.Validation
{
    /// <summary>
    /// A validation expression that indicates the value must represent a value that is not in the
    /// future.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "errorMessageAccessor: This belongs to the base class and cannot have a public accessor.")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class NotInFutureAttribute : ValidationAttribute
    {
        private readonly TemporalCheck temporalCheckType;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotInFutureAttribute"/> class.
        /// </summary>
        /// <param name="temporalCheckType">The type of check to perform.</param>
        public NotInFutureAttribute(TemporalCheck temporalCheckType)
        {
            this.temporalCheckType = temporalCheckType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotInFutureAttribute"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="temporalCheckType">The type of check to perform.</param>
        public NotInFutureAttribute(string errorMessage, TemporalCheck temporalCheckType)
                : base(errorMessage)
        {
            this.temporalCheckType = temporalCheckType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotInFutureAttribute"/> class.
        /// </summary>
        /// <param name="errorMessageAccessor">The function that enables access to validation resources.</param>
        /// <param name="temporalCheckType">The type of check to perform.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="errorMessageAccessor"/> is null.</exception>
        public NotInFutureAttribute(Func<string> errorMessageAccessor, TemporalCheck temporalCheckType)
                : base(errorMessageAccessor)
        {
            this.temporalCheckType = temporalCheckType;
        }

        /// <summary>
        /// Evaluates if the provided object is in the future or not.
        /// </summary>
        /// <param name="value">The date to evaluate.</param>
        /// <returns><c>true</c> if the date is in the future.</returns>
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is string || value is DateTime)
            {
                DateTime valueAsDateTime;

                if (DateTime.TryParse(value.ToString(), out valueAsDateTime))
                {
                    return !valueAsDateTime.IsInFuture(temporalCheckType);
                }

                return false;
            }

            throw new InvalidOperationException(Resources.InFutureAttribute_IsValid_NotDateTime_Message);
        }
    }
}
