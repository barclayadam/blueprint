using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Blueprint.Api.Errors;
using Blueprint.Api.Infrastructure;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// A middleware that will populate the API operation that is being passed through with information
    /// from the <see cref="HttpRequestMessage" />.
    /// </summary>
    public class HttpMessagePopulationMiddlewareBuilder : IMiddlewareBuilder
    {
        private static readonly JsonSerializer BodyJsonSerializer = JsonSerializer.Create(JsonApiSerializerSettings.Value);

        // ReSharper disable once MemberCanBePrivate.Global Used in generated code
        public static async Task PopulateFromMessageBody(ApiOperationContext context)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<HttpMessagePopulationMiddlewareBuilder>>();
            var request = context.Request;

            if (request.Body != null)
            {
                if (request.ContentType.Contains("application/x-www-form-urlencoded") || request.ContentType.Contains("multipart/form-data"))
                {
                    PopulateFromForm(logger, context);
                }
                else if (request.ContentType.Contains("application/json"))
                {
                    await PopulateFromJsonBodyAsync(logger, context);
                }
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global Used in generated code
        public static object GetValueFromRoute(ApiOperationContext context, string propertyName, Type propertyType)
        {
            if (!context.RouteData.TryGetValue(propertyName, out var routeValue))
            {
                return null;
            }

            try
            {
                // No conversions needed if the type is a string
                if (propertyType == typeof(string))
                {
                    return routeValue;
                }

                // A few hard-coded types that will be common in APIs are handled explicitly to avoid overhead of
                // using TypeDescriptor.GetConverter
                if (propertyType == typeof(Guid))
                {
                    return Guid.Parse((string)routeValue);
                }

                if (propertyType == typeof(int) || propertyType == typeof(int?))
                {
                    return int.Parse((string)routeValue);
                }

                if (propertyType == typeof(long) || propertyType == typeof(long?))
                {
                    return long.Parse((string)routeValue);
                }

                var typeConverter = TypeDescriptor.GetConverter(propertyType);

                return typeConverter.ConvertFrom(routeValue);
            }
            catch (Exception)
            {
                throw new NotFoundException($"Could not understand value '{routeValue}'");
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global Used in generated code
        public static void PopulateFromQueryString(ApiOperationContext context)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<HttpMessagePopulationMiddlewareBuilder>>();
            var properties = context.Descriptor.Properties;

            if (context.Request.QueryString.HasValue)
            {
                foreach (var queryParameter in context.Request.Query)
                {
                    WriteStringValues(
                        logger,
                        context.Operation,
                        properties,
                        queryParameter.Key,
                        queryParameter.Value,
                        e => throw new QueryStringParamParsingException(e.Message));
                }
            }
        }

        /// <summary>
        /// Returns <c>true</c> if <paramref name="operation"/>.<see cref="ApiOperationDescriptor.OperationType"/> has any
        /// properties, <c>false</c> otherwise (as no properties == nothing to set).
        /// </summary>
        /// <param name="operation">The operation to check against.</param>
        /// <returns>Whether to apply this middleware.</returns>
        public bool Matches(ApiOperationDescriptor operation)
        {
            return operation.Properties.Any();
        }

        public void Build(MiddlewareBuilderContext context)
        {
            var allLinks = context.Model.GetLinksForOperation(context.Descriptor.OperationType).ToList();
            var placeholderProperties = allLinks.SelectMany(l => l.Placeholders).ToList();

            // We want to find all properties that are not in _every_ link for an operation. If there are properties that could not possibly
            // be fulfilled by a route value then we know we can must try populating from body / QS, otherwise if the values should ONLY come from the
            // route data we can exclude any population of content from the HTTP body / QS
            var nonPlaceholderPropertiesCount = context.Descriptor.Properties.Length - placeholderProperties.Count(p => allLinks.All(l => l.Placeholders.Contains(p)));

            // We can completely eliminate populating from body and query string if all properties come from the router only
            if (nonPlaceholderPropertiesCount > 0)
            {
                // If the request method is a GET then there will be no body, and therefore we do not need to attempt to
                // read the message body at all.
                if (context.Descriptor.HttpMethod != HttpMethod.Get)
                {
                    context.ExecuteMethod.Frames.Add(
                        new MethodCall(typeof(HttpMessagePopulationMiddlewareBuilder), nameof(PopulateFromMessageBody)));
                }

                context.ExecuteMethod.Frames.Add(
                    new MethodCall(typeof(HttpMessagePopulationMiddlewareBuilder), nameof(PopulateFromQueryString)));
            }

            // The route value setting comes last so that it can override any values that someone may have tried to set via. other means
            // when they should not be
            var operationVariable = context.VariableFromContext(context.Descriptor.OperationType);
            var operationContextVariable = context.VariableFromContext(typeof(ApiOperationContext));

            foreach (var routeProperty in placeholderProperties)
            {
                var inAllRoutes = allLinks.All(l => l.Placeholders.Contains(routeProperty));

                if (routeProperty.PropertyType != typeof(string) && !TypeDescriptor.GetConverter(routeProperty.PropertyType).CanConvertFrom(typeof(string)))
                {
                    throw new InvalidOperationException(
                        $"Property {context.Descriptor.OperationType.Name}.{routeProperty.Name} cannot be used in routes, it cannot be converted from string");
                }

                var methodCall = $"{typeof(HttpMessagePopulationMiddlewareBuilder).FullNameInCode()}.{nameof(GetValueFromRoute)}";
                var operationProperty = operationVariable.GetProperty(routeProperty.Name);

                if (inAllRoutes)
                {
                    // Generates "operation.[Property] = ([propertyType]) HttpMessagePopulationMiddlewareBuilder.GetFromRoute(apiOperationContext, "[propertyName]", typeof([propertyType]));
                    context.ExecuteMethod.Frames.Add(new VariableSetterFrame(
                        operationProperty,
                        $"({routeProperty.PropertyType.FullNameInCode()}) {methodCall}({operationContextVariable}, \"{routeProperty.Name}\", typeof({routeProperty.PropertyType.FullNameInCode()}))"));
                }
                else
                {
                    // This is NOT a placeholder that is present in ALL links, therefore we cannot JUST override from route if it did does not exist
                    // Generates "operation.[Property] = ([propertyType]) HttpMessagePopulationMiddlewareBuilder.GetFromRoute(apiOperationContext, "[propertyName]", typeof([propertyType])) ?? operation.[Property];
                    context.ExecuteMethod.Frames.Add(new VariableSetterFrame(
                        operationProperty,
                        $"({routeProperty.PropertyType.FullNameInCode()}) {methodCall}({operationContextVariable}, \"{routeProperty.Name}\", typeof({routeProperty.PropertyType.FullNameInCode()})) ?? {operationProperty}"));
                }
            }
        }

        private static void PopulateFromForm(ILogger<HttpMessagePopulationMiddlewareBuilder> logger, ApiOperationContext context)
        {
            var request = context.Request;
            var operation = context.Operation;
            var properties = context.Descriptor.Properties;

            var form = request.Form;

            foreach (var item in form)
            {
                WriteStringValues(logger, operation, properties, item.Key, item.Value, exception => { });
            }

            if (form.Files.Any())
            {
                var props = properties.Where(x => x.PropertyType.IsAssignableFrom(typeof(IFormFile))).ToList();
                foreach (var file in form.Files)
                {
                    var propertyToWrite = props.SingleOrDefault(x => string.Equals(x.Name, file.Name, StringComparison.OrdinalIgnoreCase));
                    if (propertyToWrite == null)
                    {
                        continue;
                    }

                    propertyToWrite.SetValue(operation, file);
                }
            }
        }

        private static async Task PopulateFromJsonBodyAsync(ILogger<HttpMessagePopulationMiddlewareBuilder> logger, ApiOperationContext context)
        {
            var request = context.Request;
            var operation = context.Operation;

            var stream = request.Body;

            if (request.Body != Stream.Null && !request.Body.CanSeek)
            {
                var buffer = new MemoryStream();

                // Copy the request stream to the memory stream.
                await stream.CopyToAsync(buffer);

                // Rewind the memory stream.
                buffer.Position = 0L;

                // Replace the request stream by the memory stream.
                request.Body = buffer;
            }

            var readerFactory = context.ServiceProvider.GetRequiredService<IHttpRequestStreamReaderFactory>();

            // This is copied from JsonConvert.PopulateObject to avoid creating a new JsonSerializer on each
            // execution.
            using (var stringReader = readerFactory.CreateReader(request.Body, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(stringReader) {CloseInput = false})
            {
                try
                {
                    BodyJsonSerializer.Populate(jsonReader, operation);
                }
                catch (JsonSerializationException e)
                {
                    logger.LogDebug("Invalid body detected, malformed JSON");

                    throw new InvalidOperationException("Could not parse JSON body", e);
                }

                if (jsonReader.Read() && jsonReader.TokenType != JsonToken.Comment)
                {
                    throw new InvalidOperationException("Additional text found in JSON string after finishing deserializing object.");
                }
            }
        }

        private static void WriteStringValues(
            ILogger<HttpMessagePopulationMiddlewareBuilder> logger,
            IApiOperation operation,
            IEnumerable<PropertyInfo> properties,
            string key,
            StringValues value,
            Action<Exception> throwException)
        {
            if (value.Count > 1)
            {
                // Axios creates array queryString properties in the format: property[]=1&property[]=2
                WriteValue(logger, operation, properties, key, (string[])value, throwException);
            }
            else
            {
                WriteValue(logger, operation, properties, key, value[0], throwException);
            }
        }

        private static void WriteValue(
            ILogger<HttpMessagePopulationMiddlewareBuilder> logger,
            IApiOperation operation,
            IEnumerable<PropertyInfo> properties,
            string key,
            object value,
            Action<Exception> throwException)
        {
            key = key.Replace("[]", string.Empty);

            // PERF: No LINQ to avoid closure allocation
            foreach (var property in properties)
            {
                if (!property.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // TODO: typeof(IEnumerable).IsAssignableFrom(property.PropertyType) - support any enumerable
                if (property.PropertyType.IsArray)
                {
                    try
                    {
                        if (value.GetType().IsArray)
                        {
                            property.SetValue(operation, value);
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
                    catch (Exception e)
                    {
                        logger.LogDebug(e, "Exception when trying to write a value to message");

                        throwException(e);
                    }
                }
                else
                {
                    var typeConverter = TypeDescriptor.GetConverter(property.PropertyType);

                    if (typeConverter.CanConvertFrom(typeof(string)))
                    {
                        try
                        {
                            property.SetValue(operation, typeConverter.ConvertFrom(value));
                        }
                        catch (Exception e)
                        {
                            logger.LogDebug(e, "Exception when trying to write a value to message");

                            throwException(e);
                        }
                    }
                    else
                    {
                        logger.LogWarning("Could not convert querystring value. value={0}, property_type={1}", value, property.PropertyType);

                        throwException(new Exception($"Could not understand the value of querystring key '{key}'"));
                    }
                }
            }
        }
    }
}
