using System;
using System.Collections.Generic;

namespace Blueprint.Core.Apm
{
    /// <summary>
    /// A null <see cref="IApmTool" /> that performs no actual tracking.
    /// </summary>
    public class NullApmTool : IApmTool
    {
        /// <inheritdoc />
        public IApmSpan Start(SpanType spanType, string operationName, string type1, IDictionary<string, string> existingContext = null)
        {
            return NullApmSpan.Instance;
        }

        private class NullApmSpan : IApmSpan
        {
            public static readonly NullApmSpan Instance = new NullApmSpan();

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
        }
    }
}
