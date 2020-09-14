using System;
using System.Collections.Generic;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace Blueprint.Apm.OpenTracing
{
    /// <summary>
    /// An <see cref="IApmTool" /> that integrates with Elastic APM.
    /// </summary>
    public class OpenTracingApmTool : IApmTool
    {
        private readonly ITracer tracer;

        /// <summary>
        /// Initialises a new instance of the <see cref="OpenTracingApmTool" />.
        /// </summary>
        /// <param name="tracer">The OpenTracing tracer to use.</param>
        public OpenTracingApmTool(ITracer tracer)
        {
            this.tracer = tracer;
        }

        /// <inheritdoc />
        public IApmSpan StartOperation(ApiOperationDescriptor operation, string spanKind, IDictionary<string, string> existingContext = null)
        {
            var spanBuilder = tracer.BuildSpan("operation.execute");

            if (existingContext != null)
            {
                spanBuilder.AsChildOf(this.tracer.Extract(BuiltinFormats.TextMap, new TextMapExtractAdapter(existingContext)));
            }

            var span = spanBuilder.Start();

            Tags.Component.Set(span, operation.Name);
            Tags.SpanKind.Set(span, spanKind);

            return new OpenTracingSpan(tracer, span);
        }

        /// <inheritdoc />
        public IApmSpan Start(string spanKind, string operationName, string type, IDictionary<string, string> existingContext = null, string resourceName = null)
        {
            var spanBuilder = tracer.BuildSpan(operationName);

            if (existingContext != null)
            {
                spanBuilder.AsChildOf(this.tracer.Extract(BuiltinFormats.TextMap, new TextMapExtractAdapter(existingContext)));
            }

            var span = spanBuilder.Start();

            Tags.Component.Set(span, resourceName);
            Tags.SpanKind.Set(span, spanKind);
            span.SetTag("type", type);

            return new OpenTracingSpan(tracer, span);
        }

        private class OpenTracingSpan : IApmSpan
        {
            private readonly ITracer tracer;
            private readonly ISpan span;

            public OpenTracingSpan(ITracer tracer, ISpan span)
            {
                this.tracer = tracer;
                this.span = span;
            }

            public IApmSpan StartSpan(
                string spanKind,
                string operationName,
                string type)
            {
                return new OpenTracingSpan(
                    tracer,
                    tracer.BuildSpan(operationName)
                        .AsChildOf(span)
                        .WithTag("type", type)
                        .WithTag(Tags.SpanKind.Key, spanKind)
                        .Start());
            }

            public void Dispose()
            {
                this.span.Finish();
            }

            public void RecordException(Exception e)
            {
                Tags.Error.Set(this.span, true);

                this.span.Log(new Dictionary<string, object>
                {
                    [LogFields.Event] = "error",
                    [LogFields.ErrorObject] = e,
                });

                // We also set some additional tags directly on the span which some tools (i.e. Datadog)
                // pick up instead of the log event from above.
                this.span.SetTag("error.msg", e.Message);
                this.span.SetTag("error.stack", e.ToString());
                this.span.SetTag("error.type", e.GetType().ToString());
            }

            public void SetTag(string key, string value)
            {
                this.span.SetTag(key, value);
            }

            /// <inheritdoc />
            public void InjectContext(IDictionary<string, string> context)
            {
                this.tracer.Inject(this.tracer.ActiveSpan.Context, BuiltinFormats.TextMap, new TextMapInjectAdapter(context));
            }

            public void SetResource(string resourceName)
            {
                Tags.Component.Set(this.span, resourceName);
            }
        }
    }
}
