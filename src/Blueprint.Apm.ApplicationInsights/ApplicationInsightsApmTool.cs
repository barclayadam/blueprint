using System;
using System.Collections.Generic;
using Blueprint.Core.Apm;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

namespace Blueprint.Apm.ApplicationInsights
{
    /// <summary>
    /// An <see cref="IApmTool" /> that will push operations to Application Insights.
    /// </summary>
    public class ApplicationInsightsApmTool : IApmTool
    {
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the ApplicationInsightsApmOperationTracker class that will
        /// use the given <see cref="TelemetryClient"/> for tracking operations.
        /// </summary>
        /// <param name="telemetryClient">The client to interact with Application Insights.</param>
        public ApplicationInsightsApmTool(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        /// <inheritdoc />
        public IApmSpan Start(SpanType spanType, string operationName, string type, IDictionary<string, string> existingContext = null)
        {
            if (spanType == SpanType.Span)
            {
                var operation = telemetryClient.StartOperation<DependencyTelemetry>(operationName);
                operation.Telemetry.Type = type;

                if (existingContext != null &&
                    existingContext.TryGetValue("RootId", out var rootId) &&
                    existingContext.TryGetValue("ParentId", out var parentId))
                {
                    operation.Telemetry.Context.Operation.Id = rootId;
                    operation.Telemetry.Context.Operation.ParentId = parentId;
                }

                return new ApplicationInsightsApmSpan<DependencyTelemetry>(operation);
            }
            else
            {
                var operation = telemetryClient.StartOperation<RequestTelemetry>(operationName);
                operation.Telemetry.Properties["Type"] = type;

                if (existingContext != null &&
                    existingContext.TryGetValue("RootId", out var rootId) &&
                    existingContext.TryGetValue("ParentId", out var parentId))
                {
                    operation.Telemetry.Context.Operation.Id = rootId;
                    operation.Telemetry.Context.Operation.ParentId = parentId;
                }

                return new ApplicationInsightsApmSpan<RequestTelemetry>(operation);
            }
        }

        private class ApplicationInsightsApmSpan<T> : IApmSpan where T : OperationTelemetry
        {
            private readonly IOperationHolder<T> operation;

            public ApplicationInsightsApmSpan(IOperationHolder<T> operation)
            {
                this.operation = operation;
            }

            public void Dispose()
            {
                this.operation.Dispose();
            }

            public void RecordException(Exception e)
            {
                operation.Telemetry.Success = false;

                operation.Telemetry.Properties["ExceptionType"] = e.GetType().Name;
                operation.Telemetry.Properties["ExceptionMessage"] = e.Message;
            }

            public void SetTag(string key, string value)
            {
                operation.Telemetry.Properties[key] = value;
            }

            /// <inheritdoc />
            public void InjectContext(IDictionary<string, string> context)
            {
                context["RootId"] = operation.Telemetry.Context.Operation.Id;
                context["ParentId"] = operation.Telemetry.Id;
            }
        }
    }
}
