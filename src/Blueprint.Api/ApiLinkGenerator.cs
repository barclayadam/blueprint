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
    public class ApiLinkGenerator : IApiLinkGenerator
    {
        private static readonly char[] PathSeparatorChars = { '/' };

        private readonly string baseUri;
        private readonly ApiDataModel apiDataModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiLinkGenerator"/> class.
        /// </summary>
        /// <param name="apiConfiguration">The configuration of the API we are generating links for.</param>
        public ApiLinkGenerator(BlueprintApiOptions apiConfiguration)
        {
            Guard.NotNull(nameof(apiConfiguration), apiConfiguration);

            apiDataModel = apiConfiguration.Model;
            baseUri = apiConfiguration.BaseApiUrl.TrimEnd(PathSeparatorChars) + '/';
        }

        /// <inheritdoc />
        public Link CreateSelfLink<T>(int id, object queryString = null) where T : ApiResource
        {
            return CreateSelfLink<T>(new {id}, queryString);
        }

        /// <inheritdoc />
        public Link CreateSelfLink<T>(long id, object queryString = null) where T : ApiResource
        {
            return CreateSelfLink<T>(new {id}, queryString);
        }

        /// <inheritdoc />
        public Link CreateSelfLink<T>(string id, object queryString = null) where T : ApiResource
        {
            return CreateSelfLink<T>(new {id}, queryString);
        }

        /// <inheritdoc />
        public Link CreateSelfLink<T>(Guid id, object queryString = null) where T : ApiResource
        {
            return CreateSelfLink<T>(new {id}, queryString);
        }

        /// <inheritdoc />
        public Link CreateSelfLink<T>(object idDefinition, object queryString = null) where T : ApiResource
        {
            Guard.NotNull(nameof(idDefinition), idDefinition);

            var selfLink = apiDataModel.GetLinkFor(typeof(T), "self");

            if (selfLink == null)
            {
                throw new InvalidOperationException(
                    $"Cannot generate a self link for the resource type {typeof(T).Name} as one has not been registered. Make sure an operation link has " +
                    "been registered with the ApiDataModel of this generator with a rel of 'self', which can be achieved by using the [SelfLink] attribute on an IApiOperation.");
            }

            // baseUri always has / at end, relative never has at start
            var routeUrl = CreateRelativeUrlFromLink(selfLink, idDefinition);

            if (queryString != null)
            {
                AppendAsQueryString(routeUrl, queryString.GetType().GetProperties(), queryString, p => true);
            }

            return new Link
            {
                Href = baseUri + routeUrl,
                Type = ApiResource.GetTypeName(typeof(T)),
            };
        }

        /// <inheritdoc />
        public string CreateUrl(ApiOperationLink link, object result = null)
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

        /// <inheritdoc />
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

            // Append any _extra_ properties to the generated URL. The shouldInclude check will return true if the property to
            // be written does NOT exist as a placeholder in the link and therefore would NOT have already been "consumed"
            AppendAsQueryString(routeUrl, properties, operation, p => link.Placeholders.All(ph => ph.Property != p));

            return baseUri + routeUrl;
        }

        private static void AppendAsQueryString(StringBuilder routeUrl, PropertyInfo[] properties, object values, Func<PropertyInfo, bool> shouldInclude)
        {
            var addedQs = false;

            // Now, for every property that has a value but has NOT been placed in to the route will be added as a query string
            foreach (var property in properties)
            {
                // This property has already been handled by the route generation generation above
                if (!shouldInclude(property))
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
        }

        private static object GetDefaultValue(Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
        }

        private static StringBuilder CreateRelativeUrlFromLink(ApiOperationLink link, object result)
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
