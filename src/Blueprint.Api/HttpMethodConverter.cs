using System;
using System.Net.Http;
using Blueprint.Core.Utilities;
using Newtonsoft.Json;

namespace Blueprint.Api
{
    public class HttpMethodConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(value.ToString());
            }
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
                    var enumText = reader.Value.ToString();

                    if (enumText.Length == 0)
                    {
                        throw new JsonSerializationException("Cannot convert empty value to HttpMethod.");
                    }

                    return new HttpMethod(enumText);
                }
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException("Error converting value {0} to type '{1}'.".Fmt(reader.Value, objectType), ex);
            }

            // we don't actually expect to get here.
            throw new JsonSerializationException("Unexpected token {0} when parsing enum.".Fmt(reader.TokenType));
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HttpMethod);
        }
    }
}