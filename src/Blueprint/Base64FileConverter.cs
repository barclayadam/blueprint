using System;
using Newtonsoft.Json;

namespace Blueprint
{
    public class Base64FileConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            try
            {
                if (reader.TokenType == JsonToken.String)
                {
                    var stringData = reader.Value.ToString();
                    if (string.IsNullOrWhiteSpace(stringData))
                    {
                        throw new JsonSerializationException("Cannot convert empty value to PostedFileData.");
                    }

                    return Base64FileData.Decode(stringData);
                }
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException($"Error converting value {reader.Value} to type '{objectType}'.", ex);
            }

            // we don't actually expect to get here.
            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing PostedFileData.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Base64FileData);
        }
    }
}
