using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blueprint.Http;

/// <summary>
/// A <see cref="JsonConverterFactory" /> that can handle generic instances of <see cref="ResourceEvent" /> to (de)serialise
/// efficiently and without adding public setters / empty constructors.
/// </summary>
internal class JsonConverterFactoryForResourceEventOfT : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType
               && typeToConvert.IsAssignableTo(typeof(ResourceEvent));
    }

    public override JsonConverter? CreateConverter(
        Type typeToConvert, JsonSerializerOptions options)
    {
        var elementType = typeToConvert.GetGenericArguments()[0];

        var converter = (JsonConverter)Activator.CreateInstance(
            typeof(JsonConverterForResourceEventOfT<>)
                .MakeGenericType(elementType),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: null,
            culture: null)!;

        return converter;
    }
}

internal class JsonConverterForResourceEventOfT<T> : JsonConverter<ResourceEvent<T>> where T : ApiResource
{
    private static readonly JsonEncodedText _object = JsonEncodedText.Encode("$object");
    private static readonly JsonEncodedText _eventId = JsonEncodedText.Encode("eventId");
    private static readonly JsonEncodedText _changeType = JsonEncodedText.Encode("changeType");
    private static readonly JsonEncodedText _created = JsonEncodedText.Encode("created");
    private static readonly JsonEncodedText _resourceObject = JsonEncodedText.Encode("resourceObject");
    private static readonly JsonEncodedText _href = JsonEncodedText.Encode("href");
    private static readonly JsonEncodedText _data = JsonEncodedText.Encode("data");
    private static readonly JsonEncodedText _changedValues = JsonEncodedText.Encode("changedValues");
    private static readonly JsonEncodedText _metadata = JsonEncodedText.Encode("metadata");
    private static readonly JsonEncodedText _secureData = JsonEncodedText.Encode("secureData");
    private static readonly JsonEncodedText _correlationId = JsonEncodedText.Encode("correlationId");

    public override ResourceEvent<T> Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Unexpected end when reading JSON.");
        }

        // Ignore the fact that we have no empty constructors. We fill in all the properties anyway
        var resourceEvent = (ResourceEvent<T>)FormatterServices.GetSafeUninitializedObject(typeToConvert);

        // We do not serialise this to JSON, but given this is a typed read we can set in behind the scenes here
        resourceEvent.ResourceType = typeof(T);

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            ReadValue(ref reader, resourceEvent, options);
        }

        if (reader.TokenType != JsonTokenType.EndObject)
        {
            throw new JsonException("Unexpected end when reading JSON.");
        }

        return resourceEvent;
    }

    public override void Write(Utf8JsonWriter writer, ResourceEvent<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(_object, value.Object);
        writer.WriteString(_eventId, value.EventId);
        writer.WriteString(_changeType, value.ChangeType.ToString());
        writer.WriteString(_created, value.Created);
        writer.WriteString(_resourceObject, value.ResourceObject);

        if (value.Href != null)
        {
            writer.WriteString(_href, value.Href);
        }

        if (value.CorrelationId != null)
        {
            writer.WritePropertyName(_correlationId);
            JsonSerializer.Serialize(writer, value.CorrelationId, options);
        }

        if (value.Data != null)
        {
            writer.WritePropertyName(_data);
            JsonSerializer.Serialize(writer, value.Data, options);
        }

        if (value.ChangedValues != null)
        {
            writer.WritePropertyName(_changedValues);
            JsonSerializer.Serialize(writer, value.ChangedValues, options);
        }

        if (value.Metadata != null)
        {
            writer.WritePropertyName(_metadata);
            JsonSerializer.Serialize(writer, value.Metadata, options);
        }

        if (value.SecureData != null)
        {
            writer.WritePropertyName(_secureData);
            JsonSerializer.Serialize(writer, value.SecureData, options);
        }

        writer.WriteEndObject();
    }

    private static void ReadValue(ref Utf8JsonReader reader, ResourceEvent<T> value, JsonSerializerOptions options)
    {
        if (TryReadStringProperty(ref reader, _eventId, out var propertyValue))
        {
            value.EventId = propertyValue;
        }
        else if (TryReadStringProperty(ref reader, _changeType, out propertyValue))
        {
            value.ChangeType = Enum.Parse<ResourceEventChangeType>(propertyValue);
        }
        else if (TryReadDateTimeOffsetProperty(ref reader, _created, out var dateProperty))
        {
            value.Created = dateProperty;
        }
        else if (TryReadStringProperty(ref reader, _resourceObject, out propertyValue))
        {
            value.ResourceObject = propertyValue;
        }
        else if (TryReadStringProperty(ref reader, _href, out propertyValue))
        {
            value.Href = propertyValue;
        }
        else if (TryReadProperty<T>(ref reader, _data, options, out var dataProp))
        {
            value.Data = dataProp;
        }
        else if (TryReadDictionary(ref reader, _changedValues, options, out var changedValues))
        {
            value.ChangedValues = changedValues;
        }
        else if (TryReadDictionary(ref reader, _metadata, options, out var metadata))
        {
            value.Metadata = metadata;
        }
        else if (TryReadDictionary(ref reader, _secureData, options, out var secureData))
        {
            value.SecureData = secureData;
        }
        else if (TryReadStringProperty(ref reader, _correlationId, out propertyValue))
        {
            value.CorrelationId = propertyValue;
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

    private static bool TryReadDateTimeOffsetProperty(ref Utf8JsonReader reader, JsonEncodedText propertyName, out DateTimeOffset value)
    {
        if (!reader.ValueTextEquals(propertyName.EncodedUtf8Bytes))
        {
            value = default;
            return false;
        }

        reader.Read();
        value = reader.GetDateTimeOffset();
        return true;
    }

    private static bool TryReadProperty<T>(ref Utf8JsonReader reader, JsonEncodedText propertyName, JsonSerializerOptions? options, out T value)
    {
        if (!reader.ValueTextEquals(propertyName.EncodedUtf8Bytes))
        {
            value = default;
            return false;
        }

        reader.Read();
        value = JsonSerializer.Deserialize<T>(ref reader, options);
        return true;
    }

    private static bool TryReadDictionary(ref Utf8JsonReader reader, JsonEncodedText propertyName, JsonSerializerOptions? options, out Dictionary<string, object> value)
    {
        if (!reader.ValueTextEquals(propertyName.EncodedUtf8Bytes))
        {
            value = default;
            return false;
        }

        reader.Read();

        if (reader.TokenType == JsonTokenType.Null)
        {
            value = default;
            return true;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected an object for property {propertyName}");
        }

        var dictionary = new Dictionary<string, object>();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var key = reader.GetString();
            reader.Read();

            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    dictionary[key] = reader.GetInt32();
                    break;

                case JsonTokenType.String:
                    dictionary[key] = reader.GetString();
                    break;

                case JsonTokenType.True:
                case JsonTokenType.False:
                    dictionary[key] = reader.GetBoolean();
                    break;

                case JsonTokenType.StartObject:
                case JsonTokenType.StartArray:
                    dictionary[key] = JsonSerializer.Deserialize(ref reader, typeof(object), options);
                    break;

                default:
                    throw new JsonException();
            }
        }

        value = dictionary;
        return true;
    }
}