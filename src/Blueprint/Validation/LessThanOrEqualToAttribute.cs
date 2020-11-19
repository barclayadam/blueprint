using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Blueprint.Validation
{
    /// <summary>
    /// Requires the property this attribute is assigned to, to be a number less than or equal to the specified maximum value.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "errorMessageAccessor cannot be given a public accessor as it belogs to the base class.")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class LessThanOrEqualToAttribute : ValidationAttribute
    {
        private readonly int _maximumValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="LessThanOrEqualToAttribute"/> class.
        /// </summary>
        /// <param name="maximumValue">The minimum value allowed.</param>
        public LessThanOrEqualToAttribute(int maximumValue)
        {
            this._maximumValue = maximumValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LessThanOrEqualToAttribute"/> class.
        /// </summary>
        /// <param name="maximumValue">The maximum value allowed.</param>
        /// <param name="errorMessage">The error message.</param>
        public LessThanOrEqualToAttribute(int maximumValue, string errorMessage)
            : base(errorMessage)
        {
            this._maximumValue = maximumValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LessThanOrEqualToAttribute"/> class.
        /// </summary>
        /// <param name="maximumValue">The maximum value allowed.</param>
        /// <param name="errorMessageAccessor">The function that enables access to validation resources.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="errorMessageAccessor"/> is null.
        /// </exception>
        public LessThanOrEqualToAttribute(int maximumValue, Func<string> errorMessageAccessor)
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
            return number <= this.MaximumValue;
        }
    }
}
