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
            TValue value;

            return dictionary.TryGetValue(key, out value) ? value : dictionary[key] = valueCreator(key);
        }
        
        public static T MergeLeft<T, K, V>(this T me, params IDictionary<K, V>[] others)
                where T : IDictionary<K, V>, new()
        {
            var newMap = new T();

            foreach (var src in new List<IDictionary<K, V>> { me }.Concat(others.Where(o => o != null)))
            {
                foreach (var p in src)
                {
                    newMap[p.Key] = p.Value;
                }
            }

            return newMap;
        }
    }
}
