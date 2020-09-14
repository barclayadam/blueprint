using System;
using System.Collections.Generic;
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
        public IApmSpan StartOperation(ApiOperationDescriptor operation, string spanKind, IDictionary<string, string> existingContext = null)
        {
            var request = telemetryClient.StartOperation<RequestTelemetry>(operation.Name);

            if (existingContext != null &&
                existingContext.TryGetValue("RootId", out var rootId) &&
                existingContext.TryGetValue("ParentId", out var parentId))
            {
                request.Telemetry.Context.Operation.Id = rootId;
                request.Telemetry.Context.Operation.ParentId = parentId;
            }

            return new ApplicationInsightsApmSpan<RequestTelemetry>(this, request);
        }

        /// <inheritdoc />
        public IApmSpan Start(string spanKind, string operationName, string type, IDictionary<string, string> existingContext = null, string resourceName = null)
        {
            if (spanKind == SpanKinds.Client || spanKind == SpanKinds.Producer || spanKind == SpanKinds.Internal)
            {
                var operation = telemetryClient.StartOperation<DependencyTelemetry>(operationName);
                operation.Telemetry.Type = type;
                operation.Telemetry.Target = resourceName;

                if (existingContext != null &&
                    existingContext.TryGetValue("RootId", out var rootId) &&
                    existingContext.TryGetValue("ParentId", out var parentId))
                {
                    operation.Telemetry.Context.Operation.Id = rootId;
                    operation.Telemetry.Context.Operation.ParentId = parentId;
                }

                return new ApplicationInsightsApmSpan<DependencyTelemetry>(this, operation);
            }
            else
            {
                var operation = telemetryClient.StartOperation<RequestTelemetry>(operationName);
                operation.Telemetry.Properties["Type"] = type;
                operation.Telemetry.Properties["ResourceName"] = resourceName;

                if (existingContext != null &&
                    existingContext.TryGetValue("RootId", out var rootId) &&
                    existingContext.TryGetValue("ParentId", out var parentId))
                {
                    operation.Telemetry.Context.Operation.Id = rootId;
                    operation.Telemetry.Context.Operation.ParentId = parentId;
                }

                return new ApplicationInsightsApmSpan<RequestTelemetry>(this, operation);
            }
        }

        private class ApplicationInsightsApmSpan<T> : IApmSpan where T : OperationTelemetry
        {
            private readonly ApplicationInsightsApmTool tool;
            private readonly IOperationHolder<T> operation;

            public ApplicationInsightsApmSpan(ApplicationInsightsApmTool tool, IOperationHolder<T> operation)
            {
                this.tool = tool;
                this.operation = operation;
            }

            public void Dispose()
            {
                this.operation.Dispose();
            }

            public IApmSpan StartSpan(
                string spanKind,
                string operationName,
                string type)
            {
                return tool.Start(spanKind, operationName, type);
            }

            public void RecordException(Exception e)
            {
                operation.Telemetry.Success = false;

                operation.Telemetry.Properties["ExceptionType"] = e.GetType().Name;
                operation.Telemetry.Properties["ExceptionMessage"] = e.Message;
            }

            public void SetTag(string key, string value)
            {
                switch (key)
                {
                    case "user.id":
                        operation.Telemetry.Context.User.Id = value;
                        break;

                    case "user.account_id":
                        operation.Telemetry.Context.User.AccountId = value;
                        break;

                    default:
                        operation.Telemetry.Properties[key] = value;
                        break;
                }
            }

            /// <inheritdoc />
            public void InjectContext(IDictionary<string, string> context)
            {
                context["RootId"] = operation.Telemetry.Context.Operation.Id;
                context["ParentId"] = operation.Telemetry.Id;
            }

            public void SetResource(string resourceName)
            {
                if (operation.Telemetry is DependencyTelemetry d)
                {
                    d.Target = resourceName;
                }
                else
                {
                    operation.Telemetry.Properties["ResourceName"] = resourceName;
                }
            }
        }
    }
}
