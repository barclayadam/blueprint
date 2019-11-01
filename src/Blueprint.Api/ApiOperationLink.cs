using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Blueprint.Core;

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
        /// Returns the <see cref="Format" /> string as "{0:`Format`}" to be used in a call to string.Format, pre-built
        /// here to avoid string concatenation at link generation time.
        /// </summary>
        internal string FormatSpecifier { get; }

        public override string ToString()
        {
            return Text;
        }
    }

    public class ApiOperationLink
    {
        private static readonly Regex ParameterRegex = new Regex("{(?<propName>.*?)(:(?<alternatePropName>.*?))?(\\((?<format>.*)\\))?}", RegexOptions.Compiled);
        private static readonly Regex QueryStringRegex = new Regex("\\?.*", RegexOptions.Compiled);

        /// <summary>
        /// Constructs a link that applies at a 'system' level, can be overriden by setting
        /// <see cref="ResourceType"/>.
        /// </summary>
        /// <param name="operationDescriptor">The operation this link represents.</param>
        /// <param name="urlFormat">The URL from which this link is accessed, may be templated.</param>
        /// <param name="rel">The <strong>rel</strong>ationship this link represents.</param>
        public ApiOperationLink(ApiOperationDescriptor operationDescriptor, string urlFormat, string rel)
        {
            Guard.NotNull(nameof(operationDescriptor), operationDescriptor);
            Guard.NotNull(nameof(urlFormat), urlFormat);
            Guard.NotNull(nameof(rel), rel);

            OperationDescriptor = operationDescriptor;
            UrlFormat = urlFormat.TrimStart('/');
            Rel = rel;

            var placeholders = new List<ApiOperationLinkPlaceholder>(5);

            RoutingUrl = QueryStringRegex.Replace(
                ParameterRegex.Replace(
                    UrlFormat,
                    match =>
                    {
                        var propertyName = match.Groups["propName"].Value;
                        var property = operationDescriptor.OperationType.GetProperty(
                            propertyName,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

                        if (property == null)
                        {
                            throw new InvalidOperationException(
                                $"Property {propertyName} does not exist on operation type {operationDescriptor.OperationType.Name}.");
                        }

                        placeholders.Add(new ApiOperationLinkPlaceholder(
                            match.Value,
                            match.Index,
                            match.Length,
                            property,
                            match.Groups["alternatePropName"].Success ? match.Groups["alternatePropName"].Value : null,
                            match.Groups["format"].Success ? match.Groups["format"].Value : null));

                        return "{" + propertyName + "}";
                    }),
                string.Empty);

            Placeholders = placeholders;
        }

        /// <summary>
        /// Gets a list of property names of placeholders for this link.
        /// </summary>
        public IReadOnlyList<ApiOperationLinkPlaceholder> Placeholders { get; }

        /// <summary>
        /// Gets a URL that can be used for routing, trimming the definitions
        /// of placeholders to something simpler (i.e. turns /users/{Id:UserId}/more-details?staticQuery=true to /users/{Id}/more-details).
        /// </summary>
        /// <returns>A URL representation of the link used in routing.</returns>
        public string RoutingUrl { get; }

        /// <summary>
        /// Gets the format of the URL for this link, which will <b>not</b> start with a forward
        /// slash.
        /// </summary>
        public string UrlFormat { get; }

        public ApiOperationDescriptor OperationDescriptor { get; }

        public string Rel { get; }

        public Type ResourceType { get; set; }

        /// <summary>
        /// Gets a value indicating whether this is a 'root' link, one that does not belong to
        /// a particular resource.
        /// </summary>
        public bool IsRootLink => ResourceType == null;

        /// <summary>
        /// Gets a value indicating whether this link has any placeholders, meaning they would need to be filled in
        /// by the routing engine.
        /// </summary>
        /// <returns>Whether any placeholders exist.</returns>
        public bool HasPlaceholders()
        {
            return Placeholders.Count > 0;
        }

        public override string ToString()
        {
            return $"{ResourceType?.Name ?? "Root"}#{Rel} => {OperationDescriptor.OperationType.Name}";
        }
    }
}
