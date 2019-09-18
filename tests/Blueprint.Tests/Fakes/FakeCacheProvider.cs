using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Core.Caching;

namespace Blueprint.Tests.Fakes
{
    public class FakeCacheProvider : ICacheProvider
    {
        private readonly IDictionary<string, object> items = new Dictionary<string, object>();

        public IEnumerable CachedValues { get { return items.Values; } }

        public void Add(string key, object value, CacheOptions options)
        {
            items.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return items.ContainsKey(key);
        }

        public IEnumerable<T> GetItems<T>()
        {
            return items.Values.OfType<T>();
        }

        public object GetValue(string key)
        {
            if (ContainsKey(key))
            {
                return items[key];
            }

            return null;
        }

        public void Remove(string key)
        {
            items.Remove(key);
        }
    }
}