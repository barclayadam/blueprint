using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Blueprint
{
    public class ApiOperationLink
    {
        private static readonly Regex _parameterRegex = new Regex("{(?<propName>.*?)(:(?<alternatePropName>.*?))?(\\((?<format>.*)\\))?}", RegexOptions.Compiled);
        private static readonly Regex _queryStringRegex = new Regex("\\?.*", RegexOptions.Compiled);

        private readonly string _description;

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

            this.OperationDescriptor = operationDescriptor;
            this.UrlFormat = urlFormat.TrimStart('/');
            this.Rel = rel;

            var placeholders = new List<ApiOperationLinkPlaceholder>(5);

            this.RoutingUrl = _queryStringRegex.Replace(
                _parameterRegex.Replace(
                    this.UrlFormat,
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
                            property,
                            match.Groups["alternatePropName"].Success ? match.Groups["alternatePropName"].Value : null,
                            match.Groups["format"].Success ? match.Groups["format"].Value : null));

                        return "{" + property.Name + "}";
                    }),
                string.Empty);

            this.Placeholders = placeholders;

            // Let's precalculate this as ApiOperationLinks are expected to stay around for lifetime of the application, so may as well
            // pay this cost upfront.
            this._description = $"{this.ResourceType?.Name ?? "Root"}#{this.Rel} => {this.OperationDescriptor.OperationType.Name}";
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

        /// <summary>
        /// The descriptor this link points to (i.e. the operation this link is for).
        /// </summary>
        public ApiOperationDescriptor OperationDescriptor { get; }

        /// <summary>
        /// The <strong>rel</strong>ationship this link represents, which is the key that will be used
        /// when populating the <see cref="ILinkableResource.Links" /> dictionary.
        /// </summary>
        public string Rel { get; }

        /// <summary>
        /// The resource from which this link can be created, for example a `UserResource` value that
        /// is returned from an operation. This <b>MUST</b> be an <see cref="ILinkableResource" />.
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Gets a value indicating whether this is a 'root' link, one that does not belong to
        /// a particular resource.
        /// </summary>
        public bool IsRootLink => this.ResourceType == null;

        /// <summary>
        /// Gets a value indicating whether this link has any placeholders, meaning they would need to be filled in
        /// by the routing engine.
        /// </summary>
        /// <returns>Whether any placeholders exist.</returns>
        public bool HasPlaceholders()
        {
            return this.Placeholders.Count > 0;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this._description;
        }
    }
}
