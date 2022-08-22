using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Blueprint.Utilities;

/// <summary>
/// A set of utility methods that can be used to turn any object in to a Dictionary.
/// </summary>
public static class ObjectToDictionaryHelper
{
    /// <summary>
    /// Converts the given object in to a dictionary, including all properties as-is with no conversion
    /// and including null properties.
    /// </summary>
    /// <param name="source">The object to convert.</param>
    /// <returns>A dictionary representation of the given object.</returns>
    public static Dictionary<string, object> ToDictionary(this object source)
    {
        return source.ToDictionary<object>();
    }

    /// <summary>
    /// Converts the given object in to a dictionary, including all properties (including null values) and calling
    /// ToString of them.
    /// </summary>
    /// <param name="source">The object to convert.</param>
    /// <returns>A dictionary representation of the given object.</returns>
    public static Dictionary<string, string> ToStringDictionary(this object source)
    {
        return ToDictionary(source, (p, v) => true, v => v?.ToString());
    }

    /// <summary>
    /// Converts the given object in to a dictionary, including only properties of the specified type, or one
    /// whose type is assignable (e.g. T == object would include all properties).
    /// </summary>
    /// <param name="source">The object to convert.</param>
    /// <typeparam name="T">The type of values to extract from the object.</typeparam>
    /// <returns>A dictionary representation of the given object.</returns>
    public static Dictionary<string, T> ToDictionary<T>(this object source)
    {
        return ToDictionary(source, (p, v) => typeof(T).IsAssignableFrom(p.PropertyType), v => (T)v);
    }

    public static Dictionary<string, T> ToDictionary<T>(this object source, Func<PropertyDescriptor, object, bool> filter, Func<object, T> toValue)
    {
        if (source == null)
        {
            return new Dictionary<string, T>();
        }

        var dictionary = new Dictionary<string, T>();

        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
        {
            var o = property.GetValue(source);

            if (filter(property, o))
            {
                var key = property.Name;
                var dictionaryKey = property.Attributes[typeof(DictionaryKeyAttribute)];
                if (dictionaryKey != null)
                {
                    key = ((DictionaryKeyAttribute)dictionaryKey).Name;
                }

                dictionary.Add(key, toValue(o));
            }
        }

        return dictionary;
    }
}