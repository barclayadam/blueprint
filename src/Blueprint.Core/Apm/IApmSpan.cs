using System;
using System.Collections.Generic;

namespace Blueprint.Core.Apm
{
    /// <summary>
    /// An individual span as created from <see cref="IApmTool.Start" />.
    /// </summary>
    public interface IApmSpan : IDisposable
    {
        /// <summary>
        /// Records an exception with this span, marking it as errored.
        /// </summary>
        /// <param name="e">The exception to record.</param>
        void RecordException(Exception e);

        /// <summary>
        /// Sets a tag on this span, useful for adding data to filter and search for in APM tool
        /// of choice.
        /// </summary>
        /// <param name="key">The key of the tag.</param>
        /// <param name="value">The value of the tag.</param>
        void SetTag(string key, string value);

        /// <summary>
        /// Marks this span as errored, without recording an attached <see cref="Exception" />.
        /// </summary>
        void MarkAsError();

        /// <summary>
        /// Injects context in to the given dictionary, a way of propagating span context across boundaries
        /// by stashing some information that, when passed to <see cref="IApmTool.Start" /> as the existing context,
        /// can be rehydrated and tracked as a child of this span.
        /// </summary>
        /// <param name="context">The dictionary to populate.</param>
        void InjectContext(IDictionary<string, string> context);

        /// <summary>
        /// A resource name is useful for splitting up top-level operations (i.e. http.request, task.process) to be
        /// more specific, without an additional span (i.e. GET /me or SaveUserCommand).
        /// </summary>
        /// <param name="resourceName">The resource name of this span.</param>
        void SetResource(string resourceName);

    }
}
