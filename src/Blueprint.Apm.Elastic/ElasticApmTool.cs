using System;
using System.Collections.Generic;
using System.Linq;
using Elastic.Apm.Api;

namespace Blueprint.Apm.Elastic
{
    /// <summary>
    /// An <see cref="IApmTool" /> that will push dependencies to Elastic APM.
    /// </summary>
    public class ElasticApmTool : IApmTool
    {
        /// <inheritdoc />
        public IApmSpan StartOperation(ApiOperationDescriptor operation, string spanKind, IDictionary<string, string> existingContext = null)
        {
            if (!global::Elastic.Apm.Agent.IsConfigured)
            {
                return NullApmSpan.Instance;
            }

            var tracer = global::Elastic.Apm.Agent.Tracer;

            if (tracer.CurrentTransaction != null)
            {
                // If a transaction is already underway we want to set it's name for a more accurate picture (i.e. this is
                // a HTTP call but we want to use the operation name not the HTTP route name).
                tracer.CurrentTransaction.Name = operation.Name;

                // We will also start a new span as there may be work before and after in the framework that should
                // be tracked separately from the Blueprint processing work.
                return new ElasticSpan(
                    tracer.CurrentTransaction.StartSpan(operation.Name, ApiConstants.TypeRequest, "operation", ApiConstants.ActionExec));
            }

            DistributedTracingData? distributedTracingData = null;

            if (existingContext != null && existingContext.TryGetValue("ElasticDTD", out var d))
            {
                distributedTracingData = DistributedTracingData.TryDeserializeFromString(d);
            }

            return new ElasticSpan(tracer.StartTransaction(
                operation.Name,
                spanKind,
                distributedTracingData));
        }

        /// <inheritdoc />
        public IApmSpan Start(string spanKind, string operationName, string type, IDictionary<string, string> existingContext = null, string resourceName = null)
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

            if (tracer.CurrentTransaction == null)
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

            public string TraceId => segment.TraceId;

            public void Dispose()
            {
                this.segment.End();
            }

            public IApmSpan StartSpan(
                string spanKind,
                string operationName,
                string type)
            {
                return new ElasticSpan(this.segment.StartSpan(operationName, type));
            }

            public void RecordException(Exception e)
            {
                this.segment.CaptureException(e);

                foreach (var kvp in e.Data.Keys.Cast<string>())
                {
                    this.segment.Labels[kvp] = e.Data[kvp]?.ToString();
                }
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
