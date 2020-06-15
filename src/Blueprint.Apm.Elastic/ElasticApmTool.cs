using System;
using System.Threading.Tasks;
using Blueprint.Core.Apm;
using Elastic.Apm.Api;

namespace Blueprint.Apm.Elastic
{
    /// <summary>
    /// An <see cref="IApmTool" /> that will push dependencies to Elastic APM.
    /// </summary>
    public class ElasticApmTool : IApmTool
    {
        /// <inheritdoc />
        public Task TrackDependencyAsync(string operationName, string target, string type, string extraData, Func<IApmDependencyOperation, Task> executor)
        {
            return global::Elastic.Apm.Agent.Tracer.CurrentTransaction?.CaptureSpan(
                operationName,
                type,
                (s) => executor(new ElasticApmApmDependencyOperation(s)));
        }

        private class ElasticApmApmDependencyOperation : IApmDependencyOperation
        {
            private readonly ISpan span;

            public ElasticApmApmDependencyOperation(ISpan span)
            {
                this.span = span;
            }

            public void MarkSuccess(string resultCode)
            {
                span.End();
            }

            public void MarkFailure(string resultCode, Exception exception = null)
            {
                span.CaptureException(exception);
            }
        }
    }
}
