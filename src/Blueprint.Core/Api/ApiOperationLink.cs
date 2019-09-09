using System;
using System.Text.RegularExpressions;

namespace Blueprint.Core.Api
{
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
        }

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
            return UrlFormat.Contains("{");
        }

        /// <summary>
        /// Creates a URL that can be used for routing, trimming the definitions
        /// of placeholders to something simpler (i.e. turns /users/{Id:UserId}/more-details?staticQuery=true to /users/{Id}/more-details)
        /// </summary>
        /// <returns>A URL representation of the link used in routing.</returns>
        public string GetFormatForRouting()
        {
            // We need to strip out alternate source property name else the format
            // is incorrect for routing.
            return QueryStringRegex.Replace(
                ParameterRegex.Replace(UrlFormat,
                    match => "{" + match.Groups["propName"].Value + "}"), string.Empty);
        }

        public override string ToString()
        {
            return $"{ResourceType?.Name ?? "Root"}#{Rel} => {OperationDescriptor.OperationType.Name}";
        }
    }
}
