using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Blueprint.Compiler.Util
{
    internal class LightweightCache<TKey, TValue> : IEnumerable<TValue>
    {
        private readonly IDictionary<TKey, TValue> values;

        private Func<TKey, TValue> onMissing = key =>
        {
            var message = $"Key '{key}' could not be found";
            throw new KeyNotFoundException(message);
        };

        public LightweightCache()
        {
            values = new Dictionary<TKey, TValue>();
        }

        public Func<TKey, TValue> OnMissing
        {
            set { onMissing = value; }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!values.TryGetValue(key, out var value))
                {
                    value = onMissing(key);

                    if (value != null)
                    {
                        values[key] = value;
                    }
                }

                return value;
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

        public TValue[] GetAll()
        {
            var returnValue = new TValue[values.Count];
            values.Values.CopyTo(returnValue, 0);

            return returnValue;
        }

        public void Each(Action<TKey, TValue> action)
        {
            foreach (var pair in values)
            {
                action(pair.Key, pair.Value);
            }
        }

        private void Fill(TKey key, TValue value)
        {
            if (values.ContainsKey(key))
            {
                return;
            }

            values.Add(key, value);
        }
    }
}
