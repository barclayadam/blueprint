using System;
using System.Collections.Generic;
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
        public IApmSpan Start(SpanType spanType, string operationName, string type, IDictionary<string, string> existingContext = null)
        {
            var tracer = global::Elastic.Apm.Agent.Tracer;
            DistributedTracingData? distributedTracingData = null;

            if (existingContext != null && existingContext.TryGetValue("ElasticDTD", out var d))
            {
                distributedTracingData = DistributedTracingData.TryDeserializeFromString(d);
            }

            if (spanType == SpanType.Transaction)
            {
                return new ElasticSpan(tracer.StartTransaction(
                    operationName,
                    type,
                    distributedTracingData));
            }

            return new ElasticSpan(
                tracer.CurrentTransaction.StartSpan(operationName, type));
        }

        private class ElasticSpan : IApmSpan
        {
            private readonly IExecutionSegment segment;

            public ElasticSpan(IExecutionSegment segment)
            {
                this.segment = segment;
            }

            public void Dispose()
            {
                this.segment.End();
            }

            public void RecordException(Exception e)
            {
                this.segment.CaptureException(e);
            }

            public void SetTag(string key, string value)
            {
                this.segment.Labels[key] = value;
            }

            /// <inheritdoc />
            public void InjectContext(IDictionary<string, string> context)
            {
                context["ElasticDTD"] = segment.OutgoingDistributedTracingData.SerializeToString();
            }
        }
    }
}
