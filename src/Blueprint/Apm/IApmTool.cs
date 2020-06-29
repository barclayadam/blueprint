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
        /// Starts a span / transaction for the given <see cref="ApiOperationDescriptor"/> and the span type to use (i.e. server, consumer
        /// or internal), plus an optional existing context dictionary that is used for cross-process distributed tracing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The returned span <b>MUST</b> be disposed to be fully tracked, as it is the disposal that marks a span as
        /// completed.
        /// </para>
        /// <para>
        /// The tool is responsible for deciding whether to modify ane existing ambient span / transaction or to start a new one,
        /// depending on what makes the most sense for that particular APM tool.
        /// </para>
        /// </remarks>
        /// <param name="operation">The operation that is to be processed and tracked.</param>
        /// <param name="spanKind">The span kind (<see cref="SpanKinds" />).</param>
        /// <param name="existingContext">The optional existing context.</param>
        /// <returns>A new <see cref="IApmSpan" /> that can be further configured with tags and exception recordings.</returns>
        IApmSpan StartOperation(
            ApiOperationDescriptor operation,
            string spanKind,
            [CanBeNull] IDictionary<string, string> existingContext = null);

        /// <summary>
        /// Starts a span / transaction, with the specified name (which should be as specific as makes sense to track groups
        /// of operations), the type (to help distinguish between for example "background" dependencies, or "sql" dependencies), plus
        /// an optional existing context dictionary that is used for cross-process distributed tracing.
        /// </summary>
        /// <remarks>
        /// The returned span <b>MUST</b> be disposed to be fully tracked, as it is the disposal that marks a span as
        /// completed.
        /// </remarks>
        /// <param name="spanKind">The span kind (<see cref="SpanKinds" />).</param>
        /// <param name="operationName">The name of the operation.</param>
        /// <param name="type">The type of the operation.</param>
        /// <param name="existingContext">The optional existing context.</param>
        /// <param name="resourceName">The name of the resource this span represents.</param>
        /// <returns>A new <see cref="IApmSpan" /> that can be further configured with tags and exception recordings.</returns>
        IApmSpan Start(
            string spanKind,
            string operationName,
            string type,
            [CanBeNull] IDictionary<string, string> existingContext = null,
            [CanBeNull] string resourceName = null);
    }
}
