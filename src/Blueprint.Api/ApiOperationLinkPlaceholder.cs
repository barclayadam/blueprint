using System.Reflection;

namespace Blueprint.Api
{
    public class ApiOperationLinkPlaceholder
    {
        public ApiOperationLinkPlaceholder(string text, int index, int length, PropertyInfo property, string alternatePropertyName, string format)
        {
            Text = text;
            Index = index;
            Length = length;
            Property = property;
            AlternatePropertyName = alternatePropertyName;
            Format = format;
            FormatSpecifier = "{0:" + format + "}";
        }

        public string Text { get; }

        public int Index { get; }

        public int Length { get; }

        public PropertyInfo Property { get; }

        public string AlternatePropertyName { get; }

        public string Format { get; }

        /// <summary>
        /// Gets the <see cref="Format" /> string as "{0:`Format`}" to be used in a call to string.Format, pre-built
        /// here to avoid string concatenation at link generation time.
        /// </summary>
        internal string FormatSpecifier { get; }

        public override string ToString()
        {
            return Text;
        }
    }
}
