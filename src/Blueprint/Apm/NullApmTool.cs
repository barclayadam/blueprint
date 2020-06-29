using System.Collections.Generic;

namespace Blueprint.Apm
{
    /// <summary>
    /// A null <see cref="IApmTool" /> that performs no actual tracking.
    /// </summary>
    public class NullApmTool : IApmTool
    {
        /// <inheritdoc />
        public IApmSpan StartOperation(ApiOperationDescriptor operation, string spanKind, IDictionary<string, string> existingContext = null)
        {
            return NullApmSpan.Instance;
        }

        /// <inheritdoc />
        public IApmSpan Start(string spanKind, string operationName, string type, IDictionary<string, string> existingContext = null, string resourceName = null)
        {
            return NullApmSpan.Instance;
        }
    }
}
