using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Blueprint.Api.Middleware;
using Blueprint.Compiler.Frames;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace Blueprint.Api.Http.MessagePopulation
{
    /// <summary>
    /// A <see cref="IMessagePopulationSource" /> that will read data from the HTTP query string.
    /// </summary>
    public class HttpQueryStringMessagePopulationSource : IMessagePopulationSource
    {
        /// <summary>
        /// Returns <c>0</c>.
        /// </summary>
        public int Priority => 0;

        // ReSharper disable once MemberCanBePrivate.Global Used in generated code
        public static void PopulateFromQueryString(HttpRequest request, ApiOperationContext context)
        {
            var properties = context.Descriptor.Properties;

            if (request.QueryString.HasValue)
            {
                try
                {
                    foreach (var queryParameter in request.Query)
                    {
                        WriteStringValues(
                            context.Operation,
                            properties,
                            queryParameter.Key,
                            queryParameter.Value);
                    }
                }
                catch (Exception e)
                {
                    throw new QueryStringParamParsingException(e, e.Message);
                }
            }
        }

        /// <summary>
        /// Returns an empty enumeration, this is a "catch-all" type of source.
        /// </summary>
        /// <param name="apiDataModel">The API data model.</param>
        /// <param name="operationDescriptor">The descriptor to grab owned properties for.</param>
        /// <returns>An empty enumeration.</returns>
        public IEnumerable<PropertyInfo> GetOwnedProperties(ApiDataModel apiDataModel, ApiOperationDescriptor operationDescriptor)
        {
            return Enumerable.Empty<PropertyInfo>();
        }

        /// <inheritdoc />
        public void Build(IReadOnlyCollection<PropertyInfo> ownedProperties, MiddlewareBuilderContext context)
        {
            // We can bail early on any code generation as we know that all properties are fulfilled by
            // other sources
            if (ownedProperties.Count == context.Descriptor.Properties.Length)
            {
                return;
            }

            context.ExecuteMethod.Frames.Add(
                new MethodCall(typeof(HttpQueryStringMessagePopulationSource), nameof(PopulateFromQueryString)));
        }

        private static void WriteStringValues(
            IApiOperation operation,
            IEnumerable<PropertyInfo> properties,
            string key,
            StringValues value)
        {
            if (value.Count > 1)
            {
                // Axios creates array queryString properties in the format: property[]=1&property[]=2
                WriteValue(operation, properties, key, (string[])value);
            }
            else
            {
                WriteValue(operation, properties, key, value[0]);
            }
        }

        private static void WriteValue(
            IApiOperation operation,
            IEnumerable<PropertyInfo> properties,
            string key,
            object value)
        {
            key = key.Replace("[]", string.Empty);

            // PERF: No LINQ to avoid closure allocation
            foreach (var property in properties)
            {
                if (!property.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Short-circuit if we already have the correct value type
                if (property.PropertyType == value.GetType())
                {
                    property.SetValue(operation, value);

                    continue;
                }

                var isList = property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
                var isIEnumerable = property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>);

                if (property.PropertyType.IsArray || isIEnumerable || isList)
                {
                    if (value is string[] strArray)
                    {
                        if (isList)
                        {
                            property.SetValue(operation, strArray.ToList());
                        }
                        else
                        {
                            property.SetValue(operation, strArray);
                        }
                    }
                    else
                    {
                        var valueAsString = (string)value;
                        object propertyValue;

                        if (valueAsString.StartsWith("["))
                        {
                            propertyValue = JObject.Parse("{\"value\": " + value + "}")["value"].ToObject(property.PropertyType);
                        }
                        else
                        {
                            propertyValue = JObject.Parse("{\"value\": [\"" + value + "\"]}")["value"].ToObject(property.PropertyType);
                        }

                        property.SetValue(operation, propertyValue);
                    }
                }
                else
                {
                    var typeConverter = TypeDescriptor.GetConverter(property.PropertyType);

                    if (typeConverter.CanConvertFrom(typeof(string)))
                    {
                        property.SetValue(operation, typeConverter.ConvertFrom(value));
                    }
                    else
                    {
                        throw new Exception($"Could not understand the value of querystring key '{key}'");
                    }
                }
            }
        }
    }
}
