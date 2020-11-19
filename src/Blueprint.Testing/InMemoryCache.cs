using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Caching;

namespace Blueprint.Testing
{
    /// <summary>
    /// Implements an <see cref="ICache" /> by storing objects in an in-memory only concurrent dictionary.
    /// </summary>
    public class InMemoryCache : ICache
    {
        private readonly ConcurrentDictionary<string, object> _items = new ConcurrentDictionary<string, object>();

        /// <inheritdoc />
        public void Add<T>(string category, object key, T value)
        {
            this._items.AddOrUpdate(GenerateStorageKey<T>(key), _ => value, (_, __) => value);
        }

        /// <inheritdoc />
        public bool ContainsKey<T>(object key)
        {
            return this._items.ContainsKey(GenerateStorageKey<T>(key));
        }

        public IEnumerable<T> GetItems<T>()
        {
            return this._items.Values.OfType<T>();
        }

        /// <inheritdoc />
        public T GetValue<T>(object key)
        {
            if (this.ContainsKey<T>(key))
            {
                return (T)this._items[GenerateStorageKey<T>(key)];
            }

            return default;
        }

        /// <inheritdoc />
        public void Remove<T>(object key)
        {
            this._items.TryRemove(GenerateStorageKey<T>(key), out _);
        }

        private static string GenerateStorageKey<T>(object key)
        {
            return typeof(T).Name + key;
        }
    }
}
