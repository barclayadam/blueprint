using System;
using System.Collections.Generic;
using Blueprint.Core.Apm;
using OpenTracing;
using OpenTracing.Propagation;

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
        public IApmSpan Start(SpanType spanType, string operationName, string type, IDictionary<string, string> existingContext = null)
        {
            var spanBuilder = tracer.BuildSpan(operationName)
                .WithTag("span.kind", spanType == SpanType.Transaction ? "server" : "client")
                .WithTag("type", type);

            if (existingContext != null)
            {
                spanBuilder.AsChildOf(this.tracer.Extract(BuiltinFormats.TextMap, new TextMapExtractAdapter(existingContext)));
            }

            return new OpenTracingSpan(tracer, spanBuilder.Start());
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

            public void Dispose()
            {
                this.span.Finish();
            }

            public void RecordException(Exception e)
            {
                this.span.SetTag("error", true);

                this.span.Log(new Dictionary<string, object>
                {
                    ["event"] = "error",
                    ["message"] = e.Message,
                    ["error.kind"] = "Exception",
                    ["error.object"] = e,
                    ["error.stack"] = e.StackTrace,
                });
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
        }
    }
}
