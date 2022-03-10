using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Blueprint
{
    /// <summary>
    /// An exception that will be thrown if the format of an <see cref="ApiOperationLink" /> is invalid.
    /// </summary>
    public class OperationLinkFormatException : Exception
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="OperationLinkFormatException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public OperationLinkFormatException(string message)
            : base(message)
        {
        }
    }

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
                            throw new OperationLinkFormatException(
                                $"URL {urlFormat} is invalid. Property {propertyName} does not exist on operation type {operationDescriptor.OperationType.Name}");
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
        /// Constructs a link that applies at a resource level.
        /// </summary>
        /// <param name="operationDescriptor">The operation this link represents.</param>
        /// <param name="urlFormat">The URL from which this link is accessed, may be templated.</param>
        /// <param name="rel">The <strong>rel</strong>ationship this link represents.</param>
        /// <param name="resourceType">he resource from which this link can be created, for example a `UserResource` value that
        /// is returned from an operation. This <b>MUST</b> be an <see cref="ILinkableResource" />.</param>
        public ApiOperationLink(ApiOperationDescriptor operationDescriptor, string urlFormat, string rel, [CanBeNull] Type resourceType)
            : this(operationDescriptor, urlFormat, rel)
        {
            this.ResourceType = resourceType;

            if (resourceType != null)
            {
                if (resourceType.IsAssignableTo(typeof(ILinkableResource)) == false)
                {
                    throw new OperationLinkFormatException(
                        $"Resource type {resourceType.Name} is not assignable to {nameof(ILinkableResource)}, cannot add a link for {operationDescriptor.Name}");
                }

                foreach (var placeholder in this.Placeholders)
                {
                    var prop = resourceType.GetProperty(placeholder.Property.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

                    if (prop != null)
                    {
                        continue;
                    }

                    if (placeholder.AlternatePropertyName == null)
                    {
                        throw new OperationLinkFormatException(
                            $"Link {urlFormat} for operation {operationDescriptor.Name} specifies placeholder {placeholder.Property.Name} that cannot be found on resource {resourceType.Name}");
                    }

                    var alternateProperty = resourceType.GetProperty(
                        placeholder.AlternatePropertyName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

                    if (alternateProperty == null)
                    {
                        throw new OperationLinkFormatException(
                            $"Link {urlFormat} for operation {operationDescriptor.Name} specifies placeholder {placeholder.OriginalText}. Cannot find alternate property {placeholder.AlternatePropertyName} on resource {resourceType.Name}");
                    }
                }
            }
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
        [CanBeNull]
        public Type ResourceType { get; }

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

        /// <summary>
        /// Creates a relative URL (i.e. just the pathname) from this link, with any non-placeholder properties <b>not</b>
        /// being included.
        /// </summary>
        /// <param name="value">An object that will be used to grab values to populate placeholders with.</param>
        /// <returns>A relative URL.</returns>
        /// <exception cref="InvalidOperationException">If we cannot load placeholders values from the given values argument.</exception>
        public string CreateRelativeUrl(object value)
        {
            return this.CreateRelativeUrl(value, false);
        }

        /// <summary>
        /// Creates a relative URL (i.e. just the pathname) from this link.
        /// </summary>
        /// <param name="value">An object that will be used to grab values to populate placeholders with.</param>
        /// <param name="includeExtraPropertiesAsQueryString">Whether to included any properties from the <paramref name="value"/> that are not placeholders as query string parameters.</param>
        /// <returns>A relative URL.</returns>
        /// <exception cref="InvalidOperationException">If we cannot load placeholders values from the given values argument.</exception>
        public string CreateRelativeUrl(object value, bool includeExtraPropertiesAsQueryString)
        {
            if (value == null)
            {
                throw new InvalidOperationException(
                    $"Unable to construct relative URL for link {this._description} with a null values object as it has placeholders");
            }

            var builtUrl = new StringBuilder();
            var currentIndex = 0;

            foreach (var placeholder in this.Placeholders)
            {
                // Grab the static bit of the URL _before_ this placeholder.
                builtUrl.Append(this.UrlFormat.Substring(currentIndex, placeholder.Index - currentIndex));

                // Now skip over the actual placeholder for the next iteration
                currentIndex = placeholder.Index + placeholder.Length;

                object placeholderValue;

                if (this.OperationDescriptor.OperationType == value.GetType())
                {
                    // Do not have to deal with "alternate" names, we know the original name is correct
                    placeholderValue = placeholder.Property.GetValue(value);
                }
                else
                {
                    // We cannot use the existing PropertyInfo on placeholder because the type is different, even though they are the same name
                    var property = value
                        .GetType()
                        .GetProperty(placeholder.AlternatePropertyName ?? placeholder.Property.Name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

                    if (property == null)
                    {
                        if (placeholder.AlternatePropertyName != null)
                        {
                            throw new InvalidOperationException(
                                $"Cannot find property '{placeholder.AlternatePropertyName}' (specified as alternate name) on type '{value.GetType()}'");
                        }

                        throw new InvalidOperationException(
                            $"Cannot find property '{placeholder.Property.Name}' on type '{value.GetType()}'");
                    }

                    placeholderValue = property.GetValue(value);
                }

                // If we have a placeholder value then we must return null if it does not exist, otherwise we would build URLs
                // like /users/null if using a "safe" representation
                if (placeholderValue == null)
                {
                    return null;
                }

                if (placeholder.Format != null)
                {
                    builtUrl.Append(Uri.EscapeDataString(string.Format(placeholder.FormatSpecifier, placeholderValue)));
                }
                else
                {
                    // We do not have a format so just ToString the result. We pick a few common types to cast directly to avoid indirect
                    // call to ToString when doing it as (object).ToString()
                    switch (placeholderValue)
                    {
                        case string s:
                            builtUrl.Append(Uri.EscapeDataString(s));
                            break;
                        case Guid g:
                            builtUrl.Append(Uri.EscapeDataString(g.ToString()));
                            break;
                        case int i:
                            builtUrl.Append(Uri.EscapeDataString(i.ToString()));
                            break;
                        case long l:
                            builtUrl.Append(Uri.EscapeDataString(l.ToString()));
                            break;
                        default:
                            builtUrl.Append(Uri.EscapeDataString(placeholderValue.ToString()));
                            break;
                    }
                }
            }

            if (currentIndex < this.UrlFormat.Length)
            {
                builtUrl.Append(this.UrlFormat.Substring(currentIndex));
            }

            if (includeExtraPropertiesAsQueryString)
            {
                this.AppendExtraAsQueryString(builtUrl, value.GetType().GetProperties(), value);
            }

            return builtUrl.ToString();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this._description;
        }

        private void AppendExtraAsQueryString(
            StringBuilder routeUrl,
            PropertyInfo[] properties,
            object values)
        {
            var addedQs = false;

            // Now, for every property that has a value but has NOT been placed in to the route will be added as a query string
            foreach (var property in properties)
            {
                // This property has already been handled by the route generation generation above
                if (this.Placeholders.Any(p => p.Property == property))
                {
                    continue;
                }

                var value = property.GetValue(values, null);

                // Ignore default values, they are unnecessary to pass back through the URL
                if (value == null || Equals(GetDefaultValue(property.PropertyType), value))
                {
                    continue;
                }

                if (!addedQs)
                {
                    routeUrl.Append('?');
                    addedQs = true;
                }
                else
                {
                    routeUrl.Append('&');
                }

                routeUrl.Append(property.Name);
                routeUrl.Append('=');
                routeUrl.Append(Uri.EscapeDataString(value.ToString()));
            }
        }

        private static object GetDefaultValue(Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
        }
    }
}
