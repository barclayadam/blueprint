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
using Blueprint.Compiler.Frames;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// A middleware that will populate the API operation that is being passed through with information
    /// from the <see cref="HttpRequestMessage" />.
    /// </summary>
    public class MessagePopulationMiddlewareBuilder : IMiddlewareBuilder
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly JsonSerializer BodyJsonSerializer = JsonSerializer.Create(JsonApiSerializerSettings.Value);

        // ReSharper disable once MemberCanBePrivate.Global Used in generated code
        public static async Task PopulateFromMessageBody(ApiOperationContext context)
        {
            var request = context.Request;

            if (request.Body != null && request.ContentLength > 0)
            {
                if (request.ContentType.Contains("application/x-www-form-urlencoded"))
                {
                    PopulateFromForm(context);
                }
                else if (request.ContentType.Contains("multipart/form-data"))
                {
                    PopulateFromForm(context);
                }
                else
                {
                    await PopulateFromJsonBodyAsync(context);
                }
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global Used in generated code
        public static void PopulateFromRoute(ApiOperationContext context)
        {
            var properties = context.Descriptor.Properties;

            foreach (var routeValue in context.RouteData)
            {
                WriteValue(
                    context.Operation,
                    properties,
                    routeValue.Key,
                    routeValue.Value,
                    e => throw new NotFoundException("Route cannot be found. Check your URL format"));
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global Used in generated code
        public static void PopulateFromQueryString(ApiOperationContext context)
        {
            var properties = context.Descriptor.Properties;

            if (context.Request.QueryString.HasValue)
            {
                foreach (var queryParameter in context.Request.Query)
                {
                    WriteStringValues(
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
        /// <param name="operation"></param>
        /// <returns>Whether to apply this middleware.</returns>
        public bool Matches(ApiOperationDescriptor operation)
        {
            return operation.OperationType.GetProperties().Any();
        }

        public void Build(MiddlewareBuilderContext context)
        {
            // If the request method is a GET then there will be no body, and therefore we do not need to attempt to
            // read the message body at all.
            if (context.Descriptor.HttpMethod != HttpMethod.Get)
            {
                context.ExecuteMethod.Frames.Add(
                    new MethodCall(typeof(MessagePopulationMiddlewareBuilder), nameof(PopulateFromMessageBody)));
            }

            if (context.Model.GetLinksForOperation(context.Descriptor.OperationType).Any(l => l.HasPlaceholders()))
            {
                context.ExecuteMethod.Frames.Add(
                    new MethodCall(typeof(MessagePopulationMiddlewareBuilder), nameof(PopulateFromRoute)));
            }

            context.ExecuteMethod.Frames.Add(
                new MethodCall(typeof(MessagePopulationMiddlewareBuilder), nameof(PopulateFromQueryString)));
        }

        private static void PopulateFromForm(ApiOperationContext context)
        {
            var request = context.Request;
            var operation = context.Operation;
            var properties = context.Descriptor.Properties;

            var form = request.Form;

            foreach (var item in form)
            {
                WriteStringValues(operation, properties, item.Key, item.Value, exception => { });
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

        private static async Task PopulateFromJsonBodyAsync(ApiOperationContext context)
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

            // This is copied from JsonConvert.PopulateObject to avoid creating a new JsonSerializer on each
            // execution.
            using (var stringReader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            using (var jsonReader = new JsonTextReader(stringReader) {CloseInput = false})
            {
                try
                {
                    BodyJsonSerializer.Populate(jsonReader, operation);
                }
                catch (JsonSerializationException e)
                {
                    Log.Debug("Invalid body detected, malformed JSON");

                    throw new InvalidOperationException("Could not parse JSON body", e);
                }

                if (jsonReader.Read() && jsonReader.TokenType != JsonToken.Comment)
                {
                    throw new InvalidOperationException("Additional text found in JSON string after finishing deserializing object.");
                }
            }
        }

        private static void WriteStringValues(
            IApiOperation operation,
            IEnumerable<PropertyInfo> properties,
            string key,
            StringValues value,
            Action<Exception> throwException)
        {
            if (value.Count > 1)
            {
                // Axios creates array queryString properties in the format: property[]=1&property[]=2
                WriteValue(operation, properties, key, (string[])value, throwException);
            }
            else
            {
                WriteValue(operation, properties, key, value[0], throwException);
            }
        }

        private static void WriteValue(
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
                        Log.Debug(e, "Exception when trying to write a value to message");

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
                            Log.Debug(e, "Exception when trying to write a value to message");

                            throwException(e);
                        }
                    }
                    else
                    {
                        Log.Warn("Could not convert querystring value. value={0}, property_type={1}", value, property.PropertyType);

                        throwException(new Exception($"Could not understand the value of querystring key '{key}'"));
                    }
                }
            }
        }
    }
}
