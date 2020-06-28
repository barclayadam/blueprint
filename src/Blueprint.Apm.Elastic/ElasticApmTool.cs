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
        public IApmSpan Start(
            SpanType spanType,
            string operationName,
            string type,
            IDictionary<string, string> existingContext = null,
            string resourceName = null)
        {
            if (!global::Elastic.Apm.Agent.IsConfigured)
            {
                return NullApmSpan.Instance;
            }

            var tracer = global::Elastic.Apm.Agent.Tracer;
            DistributedTracingData? distributedTracingData = null;

            if (existingContext != null && existingContext.TryGetValue("ElasticDTD", out var d))
            {
                distributedTracingData = DistributedTracingData.TryDeserializeFromString(d);
            }

            // We special-case when we have no parent information to set and Elastic already has a
            // current transaction to instead create a child span, not a new Transaction
            if (tracer.CurrentTransaction != null && existingContext == null)
            {
                return new ElasticSpan(
                    tracer.CurrentTransaction.StartSpan(operationName, type, resourceName));
            }

            if (spanType == SpanType.Transaction)
            {
                return new ElasticSpan(tracer.StartTransaction(
                    operationName,
                    type,
                    distributedTracingData));
            }

            return new ElasticSpan(
                tracer.CurrentTransaction.StartSpan(operationName, type, resourceName));
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
                if (this.segment is ISpan span)
                {
                    switch (key)
                    {
                        case "db.type":
                            span.Context.Db ??= new Database();
                            span.Context.Db.Type = value;
                            break;

                        case "db.instance":
                            span.Context.Db ??= new Database();
                            span.Context.Db.Instance = value;
                            break;

                        case "db.statement":
                            span.Context.Db ??= new Database();
                            span.Context.Db.Statement = value;
                            break;

                        default:
                            span.Labels[key] = value;
                            break;
                    }
                }
                else
                {
                    this.segment.Labels[key] = value;
                }
            }

            /// <inheritdoc />
            public void InjectContext(IDictionary<string, string> context)
            {
                context["ElasticDTD"] = segment.OutgoingDistributedTracingData.SerializeToString();
            }

            public void SetResource(string resourceName)
            {
                if (this.segment is ISpan span)
                {
                    span.Subtype = resourceName;
                }
                else
                {
                    this.segment.Labels["resource"] = resourceName;
                }
            }
        }
    }
}
