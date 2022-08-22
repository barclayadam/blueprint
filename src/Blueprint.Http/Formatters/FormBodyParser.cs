using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace Blueprint.Http.Formatters;

/// <summary>
/// A <see cref="TextBodyParser"/> for JSON content that uses <see cref="JsonSerializer"/>.
/// </summary>
public class FormBodyParser : BodyParser
{
    private static readonly ApiExceptionFactory _invalidForm = new ApiExceptionFactory(
        "The form payload is invalid",
        "invalid_form",
        HttpStatusCode.BadRequest);

    /// <summary>
    /// Initializes a new instance of <see cref="FormBodyParser"/>.
    /// </summary>
    public FormBodyParser()
    {
        this.SupportedMediaTypes.Add(MediaTypeHeaderValues.WwwFormUrlEncoded);
        this.SupportedMediaTypes.Add(MediaTypeHeaderValues.MultipartFormData);
    }

    /// <inheritdoc/>
    public override async Task<object> ReadAsync(BodyParserContext context)
    {
        var operation = context.Instance;
        var properties = context.OperationContext.Descriptor.Properties;

        var form = await context.HttpContext.Request.ReadFormAsync();

        foreach (var item in form)
        {
            WriteStringValues(operation, properties, item.Key, item.Value, exception => throw _invalidForm.Create(exception.Message));
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

        // We have populated the object given to us, return as-is.
        return operation;
    }

    private static void WriteStringValues(
        object operation,
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
        object operation,
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
                    throwException(new Exception($"Could not understand the value of query string key '{key}'"));
                }
            }
        }
    }
}