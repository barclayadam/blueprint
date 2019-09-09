using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Blueprint.Core;

namespace Blueprint.Notifications.Templates
{
    /// <summary>
    /// Represents the values which can be used within a template.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
            Justification = "We do not want to expose this as a dictionary, it is not required for its use by clients")]
    public class TemplateValues : IEnumerable<KeyValuePair<string, object>>
    {
        private readonly IDictionary<string, object> values;

        /// <summary>
        /// Initializes a new instance of the TemplateValues class.
        /// </summary>
        public TemplateValues()
        {
            values = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the TemplateValues class using the specified
        /// dictionary as the initial values.
        /// </summary>
        /// <param name="values">The initial values to be used by this template.</param>
        public TemplateValues(IDictionary<string, object> values)
        {
            this.values = TemplateValuesParser.Parse(values);
        }

        /// <summary>
        /// Gets all keys that have been set on this template.
        /// </summary>
        public IEnumerable<string> Keys { get { return values.Keys; } }

        /// <summary>
        /// Gets all values that have been set on this template.
        /// </summary>
        public IEnumerable<object> Values { get { return values.Values; } }

        /// <summary>
        /// Gets or sets the value that is stored against the specified key.
        /// </summary>
        /// <param name="key">The non-null unique key of the value.</param>
        /// <returns>The value represented by the specified key.</returns>
        public object this[string key]
        {
            get
            {
                Guard.NotNullOrEmpty("key", key);

                return values.ContainsKey(key) ? values[key] : null;
            }

            set
            {
                Add(key, value);
            }
        }

        /// <summary>
        /// Adds the specified key / value pairing to these template values.
        /// </summary>
        /// <param name="key">The non-null unique key of the value.</param>
        /// <param name="value">The value to be set.</param>
        public void Add(string key, object value)
        {
            Guard.NotNullOrEmpty("key", key);

            values[key] = value;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}