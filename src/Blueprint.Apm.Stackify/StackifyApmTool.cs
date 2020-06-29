using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackifyLib;

namespace Blueprint.Apm.Stackify
{
    /// <summary>
    /// An <see cref="IApmTool" /> that integrates with Stackify.
    /// </summary>
    public class StackifyApmTool : IApmTool
    {
        /// <inheritdoc />
        public IApmSpan Start(
            SpanType spanType,
            string operationName,
            string type,
            IDictionary<string, string> existingContext = null,
            string resourceName = null)
        {
            // Stackify requires us to pass a Func or Action, but Blueprint uses disposables. To make this work we have
            // Stackify wait on an "empty" async method that waits on the TCS completed below, that will be trigged when the returned
            // StackifyApmSpan is disposed, or sets an exception.
            var tcs = new TaskCompletionSource<bool>();

            var dependency = spanType == SpanType.Transaction ?
                ProfileTracer.CreateAsOperation(operationName) :
                ProfileTracer.CreateAsDependency(operationName, resourceName);

            dependency.ExecAsync(async () => await tcs.Task);

            return new StackifyApmSpan(tcs);
        }

        private class StackifyApmSpan : IApmSpan
        {
            private readonly TaskCompletionSource<bool> manualSlim;

            public StackifyApmSpan(TaskCompletionSource<bool> manualSlim)
            {
                this.manualSlim = manualSlim;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                manualSlim.SetResult(true);
            }

            /// <inheritdoc />
            public void RecordException(Exception e)
            {
                manualSlim.SetException(e);
            }

            /// <inheritdoc />
            public void SetTag(string key, string value)
            {
            }

            /// <inheritdoc />
            public void InjectContext(IDictionary<string, string> context)
            {
            }

            public void SetResource(string resourceName)
            {
                // Nothing to do here
            }
        }
    }
}
