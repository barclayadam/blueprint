using System;
using System.Collections;
using System.Collections.Generic;

namespace Blueprint.Compiler.Util
{
    internal class LightweightCache<TKey, TValue> : IEnumerable<TValue>
    {
        private readonly IDictionary<TKey, TValue> values;

        private Func<TValue, TKey> getKey = arg => throw new NotImplementedException();

        private Func<TKey, TValue> onMissing = key =>
        {
            var message = $"Key '{key}' could not be found";
            throw new KeyNotFoundException(message);
        };

        public LightweightCache()
            : this(new Dictionary<TKey, TValue>())
        {
        }

        public LightweightCache(Func<TKey, TValue> onMissing)
            : this(new Dictionary<TKey, TValue>(), onMissing)
        {
        }

        public LightweightCache(IDictionary<TKey, TValue> dictionary, Func<TKey, TValue> onMissing)
            : this(dictionary)
        {
            this.onMissing = onMissing;
        }

        public LightweightCache(IDictionary<TKey, TValue> dictionary)
        {
            values = dictionary;
        }

        public Func<TKey, TValue> OnMissing
        {
            set { onMissing = value; }
        }

        public Func<TValue, TKey> GetKey
        {
            get { return getKey; }
            set { getKey = value; }
        }

        public int Count
        {
            get { return values.Count; }
        }

        public TValue First
        {
            get
            {
                foreach (var pair in values)
                {
                    return pair.Value;
                }

                return default;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;

                if (!values.TryGetValue(key, out value))
                {
                    value = onMissing(key);

                    if (value != null)
                    {
                        values[key] = value;
                    }
                }

                return value;
            }

            set
            {
                if (values.ContainsKey(key))
                {
                    values[key] = value;
                }
                else
                {
                    values.Add(key, value);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TValue>)this).GetEnumerator();
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return values.Values.GetEnumerator();
        }

        /// <summary>
        /// Guarantees that the Cache has the default value for a given key. If it does not already exist, it's created.
        /// </summary>
        /// <param name="key"></param>
        public void FillDefault(TKey key)
        {
            Fill(key, onMissing(key));
        }

        public void Fill(TKey key, TValue value)
        {
            if (values.ContainsKey(key))
            {
                return;
            }

            values.Add(key, value);
        }

        public bool TryRetrieve(TKey key, out TValue value)
        {
            value = default;

            if (values.ContainsKey(key))
            {
                value = values[key];
                return true;
            }

            return false;
        }

        public void Each(Action<TValue> action)
        {
            foreach (var pair in values)
            {
                action(pair.Value);
            }
        }

        public void Each(Action<TKey, TValue> action)
        {
            foreach (var pair in values)
            {
                action(pair.Key, pair.Value);
            }
        }

        public bool Has(TKey key)
        {
            return values.ContainsKey(key);
        }

        public bool Exists(Predicate<TValue> predicate)
        {
            var returnValue = false;

            Each(value => returnValue |= predicate(value));

            return returnValue;
        }

        public TValue Find(Predicate<TValue> predicate)
        {
            foreach (var pair in values)
            {
                if (predicate(pair.Value))
                {
                    return pair.Value;
                }
            }

            return default;
        }

        public TValue[] GetAll()
        {
            var returnValue = new TValue[Count];
            values.Values.CopyTo(returnValue, 0);

            return returnValue;
        }

        public void Remove(TKey key)
        {
            if (values.ContainsKey(key))
            {
                values.Remove(key);
            }
        }

        public void Clear()
        {
            values.Clear();
        }

        public void WithValue(TKey key, Action<TValue> action)
        {
            if (values.ContainsKey(key))
            {
                action(this[key]);
            }
        }

        public void ClearAll()
        {
            values.Clear();
        }
    }
}
