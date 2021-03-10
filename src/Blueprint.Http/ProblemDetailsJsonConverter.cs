using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blueprint.Http
{
    internal class ProblemDetailsJsonConverter : JsonConverter<ProblemDetails>
    {
        private static readonly JsonEncodedText _type = JsonEncodedText.Encode("type");
        private static readonly JsonEncodedText _title = JsonEncodedText.Encode("title");
        private static readonly JsonEncodedText _status = JsonEncodedText.Encode("status");
        private static readonly JsonEncodedText _detail = JsonEncodedText.Encode("detail");
        private static readonly JsonEncodedText _instance = JsonEncodedText.Encode("instance");

        public override ProblemDetails Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var problemDetails = new ProblemDetails();

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Unexpected end when reading JSON.");
            }

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                ReadValue(ref reader, problemDetails, options);
            }

            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException("Unexpected end when reading JSON.");
            }

            return problemDetails;
        }

        public override void Write(Utf8JsonWriter writer, ProblemDetails value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            WriteProblemDetails(writer, value, options);
            writer.WriteEndObject();
        }

        internal static void ReadValue(ref Utf8JsonReader reader, ProblemDetails value, JsonSerializerOptions options)
        {
            if (TryReadStringProperty(ref reader, _type, out var propertyValue))
            {
                value.Type = propertyValue;
            }
            else if (TryReadStringProperty(ref reader, _title, out propertyValue))
            {
                value.Title = propertyValue;
            }
            else if (TryReadStringProperty(ref reader, _detail, out propertyValue))
            {
                value.Detail = propertyValue;
            }
            else if (TryReadStringProperty(ref reader, _instance, out propertyValue))
            {
                value.Instance = propertyValue;
            }
            else if (reader.ValueTextEquals(_status.EncodedUtf8Bytes))
            {
                reader.Read();
                if (reader.TokenType == JsonTokenType.Null)
                {
                    // Nothing to do here.
                }
                else
                {
                    value.Status = reader.GetInt32();
                }
            }
            else
            {
                var key = reader.GetString();
                reader.Read();
                value.Extensions ??= new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                value.Extensions[key] = JsonSerializer.Deserialize(ref reader, typeof(object), options);
            }
        }

        private static bool TryReadStringProperty(ref Utf8JsonReader reader, JsonEncodedText propertyName, out string value)
        {
            if (!reader.ValueTextEquals(propertyName.EncodedUtf8Bytes))
            {
                value = default;
                return false;
            }

            reader.Read();
            value = reader.GetString();
            return true;
        }

        internal static void WriteProblemDetails(Utf8JsonWriter writer, ProblemDetails value, JsonSerializerOptions options)
        {
            if (value.Type != null)
            {
                writer.WriteString(_type, value.Type);
            }

            if (value.Title != null)
            {
                writer.WriteString(_title, value.Title);
            }

            if (value.Status != null)
            {
                writer.WriteNumber(_status, value.Status.Value);
            }

            if (value.Detail != null)
            {
                writer.WriteString(_detail, value.Detail);
            }

            if (value.Instance != null)
            {
                writer.WriteString(_instance, value.Instance);
            }

            if (value.Extensions != null)
            {
                foreach (var kvp in value.Extensions)
                {
                    writer.WritePropertyName(options.DictionaryKeyPolicy?.ConvertName(kvp.Key) ?? kvp.Key);
                    JsonSerializer.Serialize(writer, kvp.Value, kvp.Value?.GetType() ?? typeof(object), options);
                }
            }
        }
    }
}
