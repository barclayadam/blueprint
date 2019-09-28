using System.Collections.Generic;
using System.Linq;
using Blueprint.Core.Caching;

namespace Blueprint.Testing
{
    public class InMemoryCache : ICache
    {
        private readonly IDictionary<string, object> items = new Dictionary<string, object>();

        public void Add<T>(string category, object key, T value)
        {
            items.Add(GenerateStorageKey<T>(key), value);
        }

        public bool ContainsKey<T>(object key)
        {
            return items.ContainsKey(GenerateStorageKey<T>(key));
        }

        public IEnumerable<T> GetItems<T>()
        {
            return items.Values.OfType<T>();
        }

        public T GetValue<T>(object key)
        {
            if (ContainsKey<T>(key))
            {
                return (T)items[GenerateStorageKey<T>(key)];
            }

            return default;
        }

        public void Remove<T>(object key)
        {
            items.Remove(GenerateStorageKey<T>(key));
        }

        private string GenerateStorageKey<T>(object key)
        {
            return typeof(T).Name + key;
        }
    }
}