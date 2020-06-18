using System;
using System.Collections.Generic;
using Blueprint.Core.Apm;
using Datadog.Trace;

namespace Blueprint.Apm.DataDog
{
    /// <summary>
    /// An <see cref="IApmTool" /> that integrates with DataDog APM.
    /// </summary>
    public class DataDogApmTool : IApmTool
    {
        /// <inheritdoc />
        public IApmSpan Start(
            SpanType spanType,
            string operationName,
            string type,
            IDictionary<string, string> existingContext = null,
            string resourceName = null)
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
            scope.Span.SetTag(Tags.SpanKind, spanType == SpanType.Transaction ? "server" : "client");

            return new OpenTracingSpan(scope);
        }

        private class OpenTracingSpan : IApmSpan
        {
            private readonly Scope scope;

            public OpenTracingSpan(Scope scope)
            {
                this.scope = scope;
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

            public void MarkAsError()
            {
                this.scope.Span.Error = true;
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
