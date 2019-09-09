using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

        private static readonly Regex ParameterRegex = new Regex("{(?<propName>.*?)(:(?<alternatePropName>.*?))?(\\((?<format>.*)\\))?}", RegexOptions.Compiled);
        private static readonly Regex QueryStringRegex = new Regex("\\?.*", RegexOptions.Compiled);

        private readonly string baseUri;
        private readonly ApiDataModel apiDataModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiLinkGenerator"/> class.
        /// </summary>
        /// <param name="apiConfiguration">The configuration of the API we are generating links for.</param>
        /// <param name="apiDataModel">The data model used to find links and operations to create URLs for.</param>
        public ApiLinkGenerator(ApiConfiguration apiConfiguration, ApiDataModel apiDataModel)
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
                Type = ApiResource.GetTypeName(typeof(T))
            };
        }

        /// <summary>
        /// Creates a fully qualified URL (using <see cref="ApiConfiguration.BaseApiUrl" />) for the specified link
        /// and "result" object that is used to fill the placeholders of the link.
        /// </summary>
        /// <param name="link">The link to generate URL for.</param>
        /// <param name="result">The "result" object used to populate placeholder values of the specified link route.</param>
        /// <returns>A fully-qualified URL</returns>
        public string CreateUrlFromLink(ApiOperationLink link, object result = null)
        {
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
        /// <exception cref="InvalidOperationException">If the URL link has a malformed placeholder (i.e. the property it names cannot be found)</exception>
        /// <exception cref="InvalidOperationException">If no links / routes have been specified for the given operation.</exception>
        public string CreateUrl(IApiOperation operation)
        {
            var operationType = operation.GetType();
            var consumedProperties = new List<PropertyInfo>();
            var properties = operationType.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            var link = apiDataModel.GetLinksForOperation(operationType).FirstOrDefault();

            if (link == null)
            {
                throw new InvalidOperationException($"No links exist for the operation {operationType.FullName}.");
            }

            var routeUrl = ParameterRegex.Replace(link.UrlFormat, match =>
            {
                var propertyName = match.Groups["propName"];
                var format = match.Groups["format"].Success ? "{0:" + match.Groups["format"].Value + "}" : "{0}";

                var sourcePropertyName = propertyName.Value;

                var property = properties.SingleOrDefault(p => p.Name.Equals(sourcePropertyName, StringComparison.OrdinalIgnoreCase));

                if (property == null)
                {
                    throw new InvalidOperationException($"Cannot find property '{propertyName}' on type '{operationType.FullName}'. Definition is {match.Value}.");
                }

                consumedProperties.Add(property);

                return Uri.EscapeDataString(string.Format(format, property.GetValue(operation)));
            });

            var addedQs = false;

            // Now, for every property that has a value but has NOT been placed in to the route will be added as a query string
            foreach (var property in properties)
            {
                if (consumedProperties.Contains(property))
                {
                    continue;
                }

                var value = property.GetValue(operation, null);

                // Ignore default values, they are unnecessary to pass back through the URL
                if (value == null || object.Equals(GetDefaultValue(property.PropertyType), value))
                {
                    continue;
                }

                if (!addedQs)
                {
                    routeUrl += "?";
                    addedQs = true;
                }
                else
                {
                    routeUrl += "&";
                }

                routeUrl += property.Name + "=" + Uri.EscapeDataString(value.ToString());
            }

            return baseUri + routeUrl;
        }

        private static object GetDefaultValue(Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
        }

        private string CreateRelativeUrlFromLink(ApiOperationLink link, object result)
        {
            if (result == null)
            {
                return link.UrlFormat;
            }

            return ParameterRegex.Replace(link.UrlFormat, match =>
            {
                var propertyName = match.Groups["propName"];
                var alternatePropName = match.Groups["alternatePropName"];
                var format = match.Groups["format"].Success ? "{0:" + match.Groups["format"].Value + "}" : "{0}";

                var sourcePropertyName = alternatePropName.Success ? alternatePropName.Value : propertyName.Value;

                var property = result.GetType().GetProperty(sourcePropertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

                if (property == null)
                {
                    if (alternatePropName.Success)
                    {
                        throw new InvalidOperationException(
                            $"Cannot find property '{alternatePropName}' (specified as alternate name) on type '{result.GetType()}'. Definition is {match.Value}.");
                    }

                    throw new InvalidOperationException(
                        $"Cannot find property '{propertyName}' on type '{result.GetType()}'. Definition is {match.Value}.");
                }

                return Uri.EscapeDataString(string.Format(format, property.GetValue(result)));
            });
        }
    }
}
