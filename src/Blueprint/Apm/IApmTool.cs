using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blueprint.Apm
{
    /// <summary>
    /// An abstraction over an APM tool.
    /// </summary>
    public interface IApmTool
    {
        /// <summary>
        /// Starts a span / transaction, with the specified name (which should be as specific as makes sense to track groups
        /// of operations), the type (to help distinguish between for example "background" dependencies, or "sql" dependencies), plus
        /// an optional existing context dictionary that is used for cross-process distributed tracing.
        /// </summary>
        /// <remarks>
        /// The returned span <b>MUST</b> be disposed to be fully tracked, as it is the disposal that marks a span as
        /// completed.
        /// </remarks>
        /// <param name="spanType">The span type.</param>
        /// <param name="operationName">The name of the operation.</param>
        /// <param name="type">The type of the operation.</param>
        /// <param name="existingContext">The optional existing context.</param>
        /// <param name="resourceName">The name of the resource this span represents.</param>
        /// <returns>A new <see cref="IApmSpan" /> that can be further configured with tags and exception recordings.</returns>
        IApmSpan Start(
            SpanType spanType,
            string operationName,
            string type,
            [CanBeNull] IDictionary<string, string> existingContext = null,
            [CanBeNull] string resourceName = null);
    }
}
