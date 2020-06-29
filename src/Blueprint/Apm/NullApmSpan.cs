using System;
using System.Collections.Generic;

namespace Blueprint.Apm
{
    /// <summary>
    /// A null implementation of <see cref="IApmSpan" /> that can be used when no APM
    /// is setup.
    /// </summary>
    public class NullApmSpan : IApmSpan
    {
        /// <summary>
        /// The single instance of <see cref="NullApmSpan" /> that should be used.
        /// </summary>
        public static readonly NullApmSpan Instance = new NullApmSpan();

        private NullApmSpan()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <inheritdoc />
        public void RecordException(Exception e)
        {
        }

        /// <inheritdoc />
        public void SetTag(string key, string value)
        {
        }

        /// <inheritdoc />
        public void InjectContext(IDictionary<string, string> context)
        {
        }

        /// <inheritdoc />
        public void SetResource(string resourceName)
        {
        }
    }
}
