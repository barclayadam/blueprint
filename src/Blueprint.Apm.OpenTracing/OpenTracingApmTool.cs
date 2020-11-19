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
        private readonly ITracer _tracer;

        /// <summary>
        /// Initialises a new instance of the <see cref="OpenTracingApmTool" />.
        /// </summary>
        /// <param name="tracer">The OpenTracing tracer to use.</param>
        public OpenTracingApmTool(ITracer tracer)
        {
            this._tracer = tracer;
        }

        /// <inheritdoc />
        public IApmSpan StartOperation(ApiOperationDescriptor operation, string spanKind, IDictionary<string, string> existingContext = null)
        {
            var spanBuilder = this._tracer.BuildSpan("operation.execute");

            if (existingContext != null)
            {
                spanBuilder.AsChildOf(this._tracer.Extract(BuiltinFormats.TextMap, new TextMapExtractAdapter(existingContext)));
            }

            var span = spanBuilder.Start();

            Tags.Component.Set(span, operation.Name);
            Tags.SpanKind.Set(span, spanKind);

            return new OpenTracingSpan(this._tracer, span);
        }

        /// <inheritdoc />
        public IApmSpan Start(string spanKind, string operationName, string type, IDictionary<string, string> existingContext = null, string resourceName = null)
        {
            var spanBuilder = this._tracer.BuildSpan(operationName);

            if (existingContext != null)
            {
                spanBuilder.AsChildOf(this._tracer.Extract(BuiltinFormats.TextMap, new TextMapExtractAdapter(existingContext)));
            }

            var span = spanBuilder.Start();

            Tags.Component.Set(span, resourceName);
            Tags.SpanKind.Set(span, spanKind);
            span.SetTag("type", type);

            return new OpenTracingSpan(this._tracer, span);
        }

        private class OpenTracingSpan : IApmSpan
        {
            private readonly ITracer _tracer;
            private readonly ISpan _span;

            public OpenTracingSpan(ITracer tracer, ISpan span)
            {
                this._tracer = tracer;
                this._span = span;
            }

            public string TraceId => this._span.Context.TraceId;

            public IApmSpan StartSpan(
                string spanKind,
                string operationName,
                string type)
            {
                return new OpenTracingSpan(
                    this._tracer,
                    this._tracer.BuildSpan(operationName)
                        .AsChildOf(this._span)
                        .WithTag("type", type)
                        .WithTag(Tags.SpanKind.Key, spanKind)
                        .Start());
            }

            public void Dispose()
            {
                this._span.Finish();
            }

            public void RecordException(Exception e)
            {
                Tags.Error.Set(this._span, true);

                this._span.Log(new Dictionary<string, object>
                {
                    [LogFields.Event] = "error",
                    [LogFields.ErrorObject] = e,
                });

                // We also set some additional tags directly on the span which some tools (i.e. Datadog)
                // pick up instead of the log event from above.
                this._span.SetTag("error.msg", e.Message);
                this._span.SetTag("error.stack", e.ToString());
                this._span.SetTag("error.type", e.GetType().ToString());
            }

            public void SetTag(string key, string value)
            {
                this._span.SetTag(key, value);
            }

            /// <inheritdoc />
            public void InjectContext(IDictionary<string, string> context)
            {
                this._tracer.Inject(this._tracer.ActiveSpan.Context, BuiltinFormats.TextMap, new TextMapInjectAdapter(context));
            }

            public void SetResource(string resourceName)
            {
                Tags.Component.Set(this._span, resourceName);
            }
        }
    }
}
