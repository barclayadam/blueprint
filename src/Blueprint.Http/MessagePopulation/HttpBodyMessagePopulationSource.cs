using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Blueprint.Compiler.Frames;
using Blueprint.Http.Infrastructure;
using Blueprint.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blueprint.Http.MessagePopulation
{
    /// <summary>
    /// A <see cref="IMessagePopulationSource" /> that will read data from a HTTP message body.
    /// </summary>
    public class HttpBodyMessagePopulationSource : IMessagePopulationSource
    {
        private static readonly JsonSerializer BodyJsonSerializer = JsonSerializer.Create(JsonApiSerializerSettings.Value);

        /// <summary>
        /// Returns <c>0</c>.
        /// </summary>
        public int Priority => 0;

        // ReSharper disable once MemberCanBePrivate.Global Used in generated code
        public static async Task PopulateFromMessageBody(HttpContext httpContext, ApiOperationContext context)
        {
            var request = httpContext.Request;

            if (request.Body != null && request.ContentType != null)
            {
                if (request.ContentType.Contains("application/x-www-form-urlencoded") || request.ContentType.Contains("multipart/form-data"))
                {
                    await PopulateFromFormAsync(request, context);
                }
                else if (request.ContentType.Contains("application/json"))
                {
                    await PopulateFromJsonBodyAsync(request, context);
                }
            }
        }

        /// <summary>
        /// Returns an empty enumeration, this is a "catch-all" type of source.
        /// </summary>
        /// <param name="apiDataModel">The API data model.</param>
        /// <param name="operationDescriptor">The descriptor to grab owned properties for.</param>
        /// <returns>An empty enumeration.</returns>
        public IEnumerable<OwnedPropertyDescriptor> GetOwnedProperties(ApiDataModel apiDataModel, ApiOperationDescriptor operationDescriptor)
        {
            return Enumerable.Empty<OwnedPropertyDescriptor>();
        }

        /// <inheritdoc />
        public void Build(
            IReadOnlyCollection<OwnedPropertyDescriptor> ownedProperties,
            IReadOnlyCollection<OwnedPropertyDescriptor> ownedBySource,
            MiddlewareBuilderContext context)
        {
            // We can bail early on any code generation as we know that all properties are fulfilled by
            // other sources
            if (ownedProperties.Count == context.Descriptor.Properties.Length)
            {
                return;
            }

            // If the request method is a GET then there will be no body, and therefore we do not need to attempt to
            // read the message body at all.
            if (context.Descriptor.GetFeatureData<HttpOperationFeatureData>().HttpMethod != "GET")
            {
                context.ExecuteMethod.Frames.Add(
                    new MethodCall(typeof(HttpBodyMessagePopulationSource), nameof(PopulateFromMessageBody)));
            }
        }

        private static async Task PopulateFromFormAsync(HttpRequest request, ApiOperationContext context)
        {
            var operation = context.Operation;
            var properties = context.Descriptor.Properties;

            var form = await request.ReadFormAsync();

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

        private static async Task PopulateFromJsonBodyAsync(HttpRequest request, ApiOperationContext context)
        {
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
                            throwException(e);
                        }
                    }
                    else
                    {
                        throwException(new Exception($"Could not understand the value of querystring key '{key}'"));
                    }
                }
            }
        }
    }
}
