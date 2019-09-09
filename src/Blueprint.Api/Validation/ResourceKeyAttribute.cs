using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Blueprint.Api.Validation
{
    /// <summary>
    /// A validation expression that indicates the value must represent a resource key, a string that
    /// has a type and a GUID id (e.g. Something/5C65D124-F856-46A4-BB3F-19764429BA1F).
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "errorMessageAccessor belongs to the base class, thus cannot have a public accessor.")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ResourceKeyAttribute : RegexAttribute
    {
        private static readonly Regex ResourceKey =
                new Regex(
                        @"^[a-z][a-z0-9]+/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$",
                        RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Initializes a new instance of the ResourceKeyAttribute class.
        /// </summary>
        public ResourceKeyAttribute() : base(ResourceKey)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ResourceKeyAttribute class.
        /// </summary>
        /// <param name="errorMessage">
        /// The error message to be shown on validation failure.
        /// </param>
        public ResourceKeyAttribute(string errorMessage)
            : base(ResourceKey, errorMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ResourceKeyAttribute class.
        /// </summary>
        /// <param name="errorMessageAccessor">A function which will return the error message to be shown on failure.</param>
        public ResourceKeyAttribute(Func<string> errorMessageAccessor)
            : base(ResourceKey, errorMessageAccessor)
        {
        }
    }
}