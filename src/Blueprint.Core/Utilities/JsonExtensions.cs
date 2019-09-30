using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Blueprint.Core.Utilities
{
    public static class JsonExtensions
    {
        // TODO potentially move typed settings in here
        private static readonly JsonSerializer DefaultSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            CheckAdditionalContent = false,
        });

        private static readonly Dictionary<Type, object> EmptyArrayCache = new Dictionary<Type, object>();

        public static T FromJson<T>(this string value) where T : class
        {
            if (value == null)
            {
                return default;
            }

            return (T)FromJson(value, typeof(T), DefaultSerializer);
        }

        public static T FromJson<T>(this string value, JsonSerializer serializer) where T : class
        {
            if (value == null)
            {
                return default;
            }

            return (T)FromJson(value, typeof(T), serializer);
        }

        public static object FromJson(this string value, Type type)
        {
            if (value == null)
            {
                return default;
            }

            return FromJson(value, type, DefaultSerializer);
        }

        public static object FromJson(this string value, Type valueType, JsonSerializer serializer)
        {
            if (value == null)
            {
                return null;
            }

            // This optimises a common case of having an empty array stored and requesting it
            // via. FromJson(value, typeof(IEnumerable<>), serializer)
            if (value == "[]")
            {
                if (EmptyArrayCache.ContainsKey(valueType))
                {
                    return EmptyArrayCache[valueType];
                }

                if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    EmptyArrayCache[valueType] = Array.CreateInstance(valueType.GetGenericArguments()[0], 0);
                    return EmptyArrayCache[valueType];
                }
            }

            using (var stringReader = new StringReader(value))
            using (var reader = new JsonTextReader(stringReader))
            {
                return serializer.Deserialize(reader, valueType);
            }
        }

        public static string ToJson<T>(this T value)
        {
            return ToJson(value, typeof(T), DefaultSerializer);
        }

        public static string ToJson<T>(this T value, JsonSerializer serializer)
        {
            return ToJson(value, typeof(T), serializer);
        }

        public static string ToJson(object value, Type objectType)
        {
            return ToJson(value, objectType, DefaultSerializer);
        }

        public static string ToJson(object value, Type objectType, JsonSerializer serializer)
        {
            if (value is IDictionary asDictionary && asDictionary.Count == 0)
            {
                return "{}";
            }

            if (value is ICollection asCollection && asCollection.Count == 0)
            {
                return "[]";
            }

            using (var sw = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture))
            using (var jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = serializer.Formatting;

                serializer.Serialize(jsonWriter, value, objectType);

                return sw.ToString();
            }
        }
    }
}
