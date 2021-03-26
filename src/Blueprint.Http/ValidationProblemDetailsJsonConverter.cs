using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blueprint.Http
{
    /// <summary>
    /// A <see cref="JsonConverter" /> for <see cref="ValidationProblemDetails" />.
    /// </summary>
    internal class ValidationProblemDetailsJsonConverter : JsonConverter<ValidationProblemDetails>
    {
        private static readonly JsonEncodedText _errors = JsonEncodedText.Encode("errors");

        /// <inheritdoc/>
        public override ValidationProblemDetails Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // ValidationProblemDetails is immutable for the errors dictionary, but we know it uses by-ref so create
            // a mutable dictionary that will be populated below.
            var errors = new Dictionary<string, IEnumerable<string>>();
            var problemDetails = new ValidationProblemDetails(errors);

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Unexpected end when reading JSON.");
            }

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.ValueTextEquals(_errors.EncodedUtf8Bytes))
                {
                    var readErrors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(ref reader, options);

                    foreach (var item in readErrors)
                    {
                        errors[item.Key] = item.Value;
                    }
                }
                else
                {
                    ProblemDetailsJsonConverter.ReadValue(ref reader, problemDetails, options);
                }
            }

            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException("Unexpected end when reading JSON.");
            }

            return problemDetails;
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, ValidationProblemDetails value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            ProblemDetailsJsonConverter.WriteProblemDetails(writer, value, options);

            writer.WriteStartObject(_errors);

            if (value.Errors != null)
            {
                foreach (var kvp in value.Errors)
                {
                    writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(kvp.Key) ?? kvp.Key);
                    JsonSerializer.Serialize(writer, kvp.Value, kvp.Value?.GetType() ?? typeof(object), options);
                }
            }

            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }
}
