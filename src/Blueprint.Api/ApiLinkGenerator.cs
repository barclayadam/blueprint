using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Blueprint.Api.Configuration;
using Blueprint.Core;

namespace Blueprint.Api
{
    /// <summary>
    /// The API link generator is responsible for creating the URLs that would be used, for example, to generate
    /// the $links properties of returned resources from registered <see cref="ApiOperationLink"/>s.
    /// </summary>
    public class ApiLinkGenerator
    {
        private static readonly char[] PathSeparatorChars = { '/' };

        private readonly string baseUri;
        private readonly ApiDataModel apiDataModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiLinkGenerator"/> class.
        /// </summary>
        /// <param name="apiConfiguration">The configuration of the API we are generating links for.</param>
        /// <param name="apiDataModel">The data model used to find links and operations to create URLs for.</param>
        public ApiLinkGenerator(BlueprintApiOptions apiConfiguration, ApiDataModel apiDataModel)
        {
            Guard.NotNull(nameof(apiConfiguration), apiConfiguration);
            Guard.NotNull(nameof(apiDataModel), apiDataModel);

            this.apiDataModel = apiDataModel;
            baseUri = apiConfiguration.BaseApiUrl.TrimEnd(PathSeparatorChars) + '/';
        }

        /// <summary>
        /// Creates the "self" link for the resource type represented by <typeparamref name="T" />, filling in
        /// the URL placeholders with values from the <paramref name="idDefinition" /> parameter.
        /// </summary>
        /// <param name="idDefinition">An object that contains properties used to fill the link (typically the ApiResource represented by the links'
        /// <see cref="ApiOperationLink.ResourceType"/> property specified as <typeparamref name="T" />).
        /// </param>
        /// <typeparam name="T">The resource type.</typeparam>
        /// <returns>A Link representing 'self' for the given resource type.</returns>
        public Link CreateSelfLink<T>(object idDefinition) where T : ApiResource
        {
            var selfLink = apiDataModel.GetLinkFor(typeof(T), "self");

            if (selfLink == null)
            {
                return null;
            }

            return new Link
            {
                Href = CreateUrlFromLink(selfLink, idDefinition),
                Type = ApiResource.GetTypeName(typeof(T)),
            };
        }

        /// <summary>
        /// Creates a fully qualified URL (using <see cref="BlueprintApiOptions.BaseApiUrl" />) for the specified link
        /// and "result" object that is used to fill the placeholders of the link.
        /// </summary>
        /// <param name="link">The link to generate URL for.</param>
        /// <param name="result">The "result" object used to populate placeholder values of the specified link route.</param>
        /// <returns>A fully-qualified URL.</returns>
        public string CreateUrlFromLink(ApiOperationLink link, object result = null)
        {
            // This duplicates the checks in CreateRelativeUrlFromLink for the purpose of not creating a new instance
            // of StringBuilder unnecessarily
            if (result == null)
            {
                return baseUri + link.UrlFormat;
            }

            // We can short-circuit in the (relatively uncommon case) of no placeholders
            if (!link.HasPlaceholders())
            {
                return baseUri + link.UrlFormat;
            }

            // baseUri always has / at end, relative never has at start
            return baseUri + CreateRelativeUrlFromLink(link, result);
        }

        /// <summary>
        /// Given a populated <see cref="IApiOperation" /> will generate a fully-qualified URL that, when hit, would execute the operation
        /// with the specified values.
        /// </summary>
        /// <remarks>
        /// This will use the <em>first</em> link (route) specified for the operation.
        /// </remarks>
        /// <param name="operation">The operation to generate a URL for.</param>
        /// <returns>A fully-qualified URL that, if hit, would execute the passed in operation with the same values.</returns>
        /// <exception cref="InvalidOperationException">If the URL link has a malformed placeholder (i.e. the property it names cannot be found).</exception>
        /// <exception cref="InvalidOperationException">If no links / routes have been specified for the given operation.</exception>
        public string CreateUrl(IApiOperation operation)
        {
            var operationType = operation.GetType();
            var operationDescriptor = apiDataModel.Operations.Single(o => o.OperationType == operationType);
            var properties = operationDescriptor.Properties;
            var link = apiDataModel.GetLinksForOperation(operationType).FirstOrDefault();

            if (link == null)
            {
                throw new InvalidOperationException($"No links exist for the operation {operationType.FullName}.");
            }

            var routeUrl = CreateRelativeUrlFromLink(link, operation);

            var addedQs = false;

            // Now, for every property that has a value but has NOT been placed in to the route will be added as a query string
            foreach (var property in properties)
            {
                // This property has already been handled by the route generation generation above
                if (link.Placeholders.Any(p => p.Property == property))
                {
                    continue;
                }

                var value = property.GetValue(operation, null);

                // Ignore default values, they are unnecessary to pass back through the URL
                if (value == null || Equals(GetDefaultValue(property.PropertyType), value))
                {
                    continue;
                }

                if (!addedQs)
                {
                    routeUrl.Append("?");
                    addedQs = true;
                }
                else
                {
                    routeUrl.Append("&");
                }

                routeUrl.Append(property.Name);
                routeUrl.Append('=');
                routeUrl.Append(Uri.EscapeDataString(value.ToString()));
            }

            return baseUri + routeUrl;
        }

        private static object GetDefaultValue(Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
        }

        private StringBuilder CreateRelativeUrlFromLink(ApiOperationLink link, object result)
        {
            if (result == null)
            {
                return new StringBuilder(link.UrlFormat);
            }

            // We can short-circuit in the (relatively uncommon case) of no placeholders
            if (!link.HasPlaceholders())
            {
                return new StringBuilder(link.UrlFormat);
            }

            var builtUrl = new StringBuilder();
            var currentIndex = 0;

            foreach (var placeholder in link.Placeholders)
            {
                // Grab the static bit of the URL _before_ this placeholder.
                builtUrl.Append(link.UrlFormat.Substring(currentIndex, placeholder.Index - currentIndex));

                // Now skip over the actual placeholder for the next iteration
                currentIndex = placeholder.Index + placeholder.Length;

                object placeholderValue;

                if (link.OperationDescriptor.OperationType == result.GetType())
                {
                    // Do not have to deal with "alternate" names, we know the original name is correct
                    placeholderValue = placeholder.Property.GetValue(result);
                }
                else
                {
                    // We cannot use the existing PropertyInfo on placeholder because the type is different, even though they are the same name
                    var property = result
                        .GetType()
                        .GetProperty(placeholder.AlternatePropertyName ?? placeholder.Property.Name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

                    if (property == null)
                    {
                        if (placeholder.AlternatePropertyName != null)
                        {
                            throw new InvalidOperationException(
                                $"Cannot find property '{placeholder.AlternatePropertyName}' (specified as alternate name) on type '{result.GetType()}'");
                        }

                        throw new InvalidOperationException(
                            $"Cannot find property '{placeholder.Property.Name}' on type '{result.GetType()}'");
                    }

                    placeholderValue = property.GetValue(result);
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

            if (currentIndex < link.UrlFormat.Length)
            {
                builtUrl.Append(link.UrlFormat.Substring(currentIndex));
            }

            return builtUrl;
        }
    }
}
