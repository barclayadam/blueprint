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
        public IApmSpan StartOperation(ApiOperationDescriptor operation, string spanKind, IDictionary<string, string> existingContext = null)
        {
            // Stackify requires us to pass a Func or Action, but Blueprint uses disposables. To make this work we have
            // Stackify wait on an "empty" async method that waits on the TCS completed below, that will be trigged when the returned
            // StackifyApmSpan is disposed, or sets an exception.
            var tcs = new TaskCompletionSource<bool>();

            var dependency = ProfileTracer.CreateAsOperation(operation.Name);

            dependency.ExecAsync(async () => await tcs.Task);

            return new StackifyApmSpan(this, tcs);
        }

        /// <inheritdoc />
        public IApmSpan Start(string spanKind, string operationName, string type, IDictionary<string, string> existingContext = null, string resourceName = null)
        {
            // Stackify requires us to pass a Func or Action, but Blueprint uses disposables. To make this work we have
            // Stackify wait on an "empty" async method that waits on the TCS completed below, that will be trigged when the returned
            // StackifyApmSpan is disposed, or sets an exception.
            var tcs = new TaskCompletionSource<bool>();

            var dependency = spanKind == SpanKinds.Server || spanKind == SpanKinds.Consumer ?
                ProfileTracer.CreateAsOperation(operationName) :
                ProfileTracer.CreateAsDependency(operationName, resourceName);

            dependency.ExecAsync(async () => await tcs.Task);

            return new StackifyApmSpan(this, tcs);
        }

        private class StackifyApmSpan : IApmSpan
        {
            private readonly StackifyApmTool _tool;
            private readonly TaskCompletionSource<bool> _manualSlim;

            public StackifyApmSpan(StackifyApmTool tool, TaskCompletionSource<bool> manualSlim)
            {
                this._tool = tool;
                this._manualSlim = manualSlim;
            }

            public string TraceId => "-";

            /// <inheritdoc />
            public IApmSpan StartSpan(
                string spanKind,
                string operationName,
                string type)
            {
                return this._tool.Start(spanKind, operationName, type);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                this._manualSlim.SetResult(true);
            }

            /// <inheritdoc />
            public void RecordException(Exception e)
            {
                this._manualSlim.SetException(e);
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
