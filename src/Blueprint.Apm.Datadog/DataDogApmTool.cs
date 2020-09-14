using System;
using System.Collections.Generic;
using Datadog.Trace;

namespace Blueprint.Apm.DataDog
{
    /// <summary>
    /// An <see cref="IApmTool" /> that integrates with DataDog APM.
    /// </summary>
    public class DataDogApmTool : IApmTool
    {
        /// <inheritdoc />
        public IApmSpan StartOperation(ApiOperationDescriptor operation, string spanKind, IDictionary<string, string> existingContext = null)
        {
            SpanContext parent = null;

            if (existingContext != null &&
                existingContext.TryGetValue("SpanId", out var spanId) &&
                existingContext.TryGetValue("TraceId", out var traceId) &&
                existingContext.TryGetValue("SamplingPriority", out var samplingPriority))
            {
                parent = new SpanContext(
                    ulong.Parse(traceId),
                    ulong.Parse(spanId),
                    Enum.TryParse<SamplingPriority>(samplingPriority, out var p) ? p : SamplingPriority.AutoKeep);
            }

            var scope = Tracer.Instance.StartActive("operation.execute", parent);

            scope.Span.Type = "request";
            scope.Span.ResourceName = operation.Name;
            scope.Span.SetTag(Tags.SpanKind, spanKind);

            return new OpenTracingSpan(this, scope);
        }

        /// <inheritdoc />
        public IApmSpan Start(string spanKind, string operationName, string type, IDictionary<string, string> existingContext = null, string resourceName = null)
        {
            SpanContext parent = null;

            if (existingContext != null &&
                existingContext.TryGetValue("SpanId", out var spanId) &&
                existingContext.TryGetValue("TraceId", out var traceId) &&
                existingContext.TryGetValue("SamplingPriority", out var samplingPriority))
            {
                parent = new SpanContext(
                    ulong.Parse(traceId),
                    ulong.Parse(spanId),
                    Enum.TryParse<SamplingPriority>(samplingPriority, out var p) ? p : SamplingPriority.AutoKeep);
            }

            var scope = Tracer.Instance.StartActive(operationName, parent);

            scope.Span.Type = type;
            scope.Span.ResourceName = resourceName ?? scope.Span.ResourceName;
            scope.Span.SetTag(Tags.SpanKind, spanKind);

            return new OpenTracingSpan(this, scope);
        }

        private class OpenTracingSpan : IApmSpan
        {
            private readonly DataDogApmTool tool;
            private readonly Scope scope;

            public OpenTracingSpan(DataDogApmTool tool, Scope scope)
            {
                this.tool = tool;
                this.scope = scope;
            }

            public IApmSpan StartSpan(
                string spanKind,
                string operationName,
                string type)
            {
                return tool.Start(spanKind, operationName, type);
            }

            public void Dispose()
            {
                this.scope.Dispose();
            }

            public void RecordException(Exception e)
            {
                this.scope.Span.SetException(e);
            }

            public void SetTag(string key, string value)
            {
                this.scope.Span.SetTag(key, value);
            }

            /// <inheritdoc />
            public void InjectContext(IDictionary<string, string> context)
            {
                context["SpanId"] = this.scope.Span.SpanId.ToString();
                context["TraceId"] = this.scope.Span.TraceId.ToString();
                context["SamplingPriority"] = this.scope.Span.GetTag(Tags.SamplingPriority);
            }

            public void SetResource(string resourceName)
            {
                this.scope.Span.ResourceName = resourceName;
            }
        }
    }
}
