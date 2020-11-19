using System;
using System.ComponentModel.DataAnnotations;

namespace Blueprint.Validation
{
    /// <summary>
    /// Requires the property this attribute is assigned to, to be a number less than the specified maximum value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class LessThanAttribute : ValidationAttribute
    {
        private readonly int _maximumValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="LessThanAttribute"/> class.
        /// </summary>
        /// <param name="maximumValue">The minimum value allowed.</param>
        public LessThanAttribute(int maximumValue)
        {
            this._maximumValue = maximumValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LessThanAttribute"/> class.
        /// </summary>
        /// <param name="maximumValue">The maximum value allowed.</param>
        /// <param name="errorMessage">The error message.</param>
        public LessThanAttribute(int maximumValue, string errorMessage)
                : base(errorMessage)
        {
            this._maximumValue = maximumValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LessThanAttribute"/> class.
        /// </summary>
        /// <param name="maximumValue">The maximum value allowed.</param>
        /// <param name="errorMessageAccessor">The function that enables access to validation resources.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="errorMessageAccessor"/> is null.
        /// </exception>
        public LessThanAttribute(int maximumValue, Func<string> errorMessageAccessor)
                : base(errorMessageAccessor)
        {
            this._maximumValue = maximumValue;
        }

        /// <summary>
        /// Gets the maximum value for the property.
        /// </summary>
        public int MaximumValue => this._maximumValue;

        /// <summary>
        /// If the value is less than that provided by the constructor, this returns true. Null objects also evaluate to true.
        /// </summary>
        /// <param name="value">
        /// The value to check.
        /// </param>
        /// <returns>
        /// True if the value is less than the minimum value, else false.
        /// </returns>
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            var number = double.Parse(value.ToString());
            return number < this.MaximumValue;
        }
    }
}
