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
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the ApplicationInsightsApmOperationTracker class that will
        /// use the given <see cref="TelemetryClient"/> for tracking operations.
        /// </summary>
        /// <param name="telemetryClient">The client to interact with Application Insights.</param>
        public ApplicationInsightsApmTool(TelemetryClient telemetryClient)
        {
            this._telemetryClient = telemetryClient;
        }

        /// <inheritdoc />
        public IApmSpan StartOperation(ApiOperationDescriptor operation, string spanKind, IDictionary<string, string> existingContext = null)
        {
            var request = this._telemetryClient.StartOperation<RequestTelemetry>(operation.Name);

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
                var operation = this._telemetryClient.StartOperation<DependencyTelemetry>(operationName);
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
                var operation = this._telemetryClient.StartOperation<RequestTelemetry>(operationName);
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
            private readonly ApplicationInsightsApmTool _tool;
            private readonly IOperationHolder<T> _operation;

            public ApplicationInsightsApmSpan(ApplicationInsightsApmTool tool, IOperationHolder<T> operation)
            {
                this._tool = tool;
                this._operation = operation;
            }

            public string TraceId => this._operation.Telemetry.Id;

            public void Dispose()
            {
                this._operation.Dispose();
            }

            public IApmSpan StartSpan(
                string spanKind,
                string operationName,
                string type)
            {
                return this._tool.Start(spanKind, operationName, type);
            }

            public void RecordException(Exception e)
            {
                this._operation.Telemetry.Success = false;

                this._operation.Telemetry.Properties["ExceptionType"] = e.GetType().Name;
                this._operation.Telemetry.Properties["ExceptionMessage"] = e.Message;
            }

            public void SetTag(string key, string value)
            {
                switch (key)
                {
                    case "user.id":
                        this._operation.Telemetry.Context.User.Id = value;
                        break;

                    case "user.account_id":
                        this._operation.Telemetry.Context.User.AccountId = value;
                        break;

                    default:
                        this._operation.Telemetry.Properties[key] = value;
                        break;
                }
            }

            /// <inheritdoc />
            public void InjectContext(IDictionary<string, string> context)
            {
                context["RootId"] = this._operation.Telemetry.Context.Operation.Id;
                context["ParentId"] = this._operation.Telemetry.Id;
            }

            public void SetResource(string resourceName)
            {
                if (this._operation.Telemetry is DependencyTelemetry d)
                {
                    d.Target = resourceName;
                }
                else
                {
                    this._operation.Telemetry.Properties["ResourceName"] = resourceName;
                }
            }
        }
    }
}
