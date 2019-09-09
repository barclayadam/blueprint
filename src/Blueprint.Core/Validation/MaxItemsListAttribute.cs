using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Blueprint.Core.Validation
{
    /// <summary>
    /// Requies that the collection this attribute is assigned to has fewer than the maximum number of items specified.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "errorMessageAccessor belongs to the base class, thus cannot have a public accessor.")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class MaxItemsListAttribute : ValidationAttribute
    {
        private readonly int maxItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxItemsListAttribute"/> class.
        /// </summary>
        /// <param name="maxItems">The maximum number of items.</param>
        public MaxItemsListAttribute(int maxItems)
        {
            this.maxItems = maxItems;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxItemsListAttribute"/> class.
        /// </summary>
        /// <param name="maxItems">The maximum number of items.</param>
        /// <param name="errorMessage">The error message.</param>
        public MaxItemsListAttribute(int maxItems, string errorMessage)
                : base(errorMessage)
        {
            this.maxItems = maxItems;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxItemsListAttribute"/> class.
        /// </summary>
        /// <param name="maxItems">The maximum number of items.</param>
        /// <param name="errorMessageAccessor">The function that enables access to validation resources.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="errorMessageAccessor"/> is null.</exception>
        public MaxItemsListAttribute(int maxItems, Func<string> errorMessageAccessor)
                : base(errorMessageAccessor)
        {
            this.maxItems = maxItems;
        }

        /// <summary>
        /// Gets the maximum number of items allowed in the collection.
        /// </summary>
        public int MaxItems { get { return maxItems; } }

        /// <summary>
        /// The property is valid if it is a collection with fewer than the specified number of items.
        /// </summary>
        /// <param name="value">An IEnumerable instance.</param>
        /// <returns><c>true</c> if the passed object contains fewer than the speficied number of items.</returns>
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is IEnumerable)
            {
                return ((IEnumerable<object>)value).Count() <= MaxItems;
            }

            throw new InvalidOperationException("This attribute can only be applied to a property whose type implements IEnumerable.");
        }
    }
}