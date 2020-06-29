using System.Reflection;

namespace Blueprint
{
    /// <summary>
    /// Represents a single "placeholder" within the text of an <see cref="ApiOperationLink" />, defining
    /// a "slot" that would be filled by a client to pass through a parameter (i.e. /groups/{Id} would have a single
    /// placeholder named Id).
    /// </summary>
    public class ApiOperationLinkPlaceholder
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="ApiOperationLinkPlaceholder" /> class.
        /// </summary>
        /// <param name="originalText">The original text specified for this placeholder.</param>
        /// <param name="index">The index in to the url of the parent <see cref="ApiOperationLink"/>.</param>
        /// <param name="property">The property the placeholder represents.</param>
        /// <param name="alternatePropertyName">An alternate property name, used when constructing links from resources
        /// that have a property representing the same property named differently.</param>
        /// <param name="format">A format for the placeholder value, used when constructing a URL from this link and a
        /// "resource".</param>
        public ApiOperationLinkPlaceholder(string originalText, int index, PropertyInfo property, string alternatePropertyName, string format)
        {
            OriginalText = originalText;
            Index = index;
            Length = originalText.Length;
            Property = property;
            AlternatePropertyName = alternatePropertyName;
            Format = format;
            FormatSpecifier = "{0:" + format + "}";
        }

        /// <summary>
        /// The original text of this placeholder, taken without modification from the <see cref="ApiOperationLink" />.
        /// </summary>
        public string OriginalText { get; }

        /// <summary>
        /// The 0-based index in to the original <see cref="ApiOperationLink.UrlFormat" /> string.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The length of the <see cref="OriginalText" />.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// The <see cref="PropertyInfo" /> this represents, the property that would be filled by routing
        /// when populating an <see cref="IApiOperation" />.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// The name of a property that can be used when creating links from resources that may have a different
        /// property name to the <see cref="IApiOperation" />.
        /// </summary>
        public string AlternatePropertyName { get; }

        /// <summary>
        /// A format string (used in <see cref="object.ToString" />) to format a value when constructing a new
        /// URL.
        /// </summary>
        public string Format { get; }

        /// <summary>
        /// Gets the <see cref="Format" /> string as "{0:`Format`}" to be used in a call to string.Format, pre-built
        /// here to avoid string concatenation at link generation time.
        /// </summary>
        public string FormatSpecifier { get; }

        /// <summary>
        /// Returns <see cref="OriginalText" />.
        /// </summary>
        /// <returns><see cref="OriginalText"/>.</returns>
        public override string ToString()
        {
            return OriginalText;
        }
    }
}
