using System;
using System.Collections.Generic;
using System.Linq;

namespace Blueprint.Core.Utilities
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(
                this IDictionary<TKey, TValue> dictionary,
                TKey key,
                Func<TKey, TValue> valueCreator)
        {
            return dictionary.TryGetValue(key, out var value) ? value : dictionary[key] = valueCreator(key);
        }
    }
}
