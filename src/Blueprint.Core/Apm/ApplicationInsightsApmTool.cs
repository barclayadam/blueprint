using System;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Blueprint.Core.Apm
{
    /// <summary>
    /// An <see cref="IApmTool" /> that will push operations to Application Insights.
    /// </summary>
    public class ApplicationInsightsApmTool : IApmTool
    {
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the ApplicationInsightsApmOperationTracker class that will
        /// use the given <see cref="TelemetryClient"/> for tracking operation.s
        /// </summary>
        /// <param name="telemetryClient"></param>
        public ApplicationInsightsApmTool(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        /// <inheritdoc />
        public async Task InvokeAsync(string operationName, Func<Task> executor)
        {
            var requestTelemetry = new RequestTelemetry
            {
                Name = operationName
            };

            var operation = telemetryClient.StartOperation(requestTelemetry);

            try
            {
                await executor();
            }
            catch (Exception)
            {
                requestTelemetry.Success = false;
                telemetryClient.StopOperation(operation);

                throw;
            }
            finally
            {
                requestTelemetry.Success = true;
                telemetryClient.StopOperation(operation);
            }
        }

        /// <inheritdoc />
        public async Task TrackDependencyAsync(string operationName, string target, string type, string extraData, Func<IApmDependencyOperation, Task> executor)
        {
            using (var operation = telemetryClient.StartOperation<DependencyTelemetry>(operationName))
            {
                operation.Telemetry.Target = target;
                operation.Telemetry.Type = type;
                operation.Telemetry.Data = extraData;

                var op = new ApplicationInsightsApmDependencyOperation(operation);

                await executor(op);
            }
        }

        private class ApplicationInsightsApmDependencyOperation : IApmDependencyOperation
        {
            private readonly IOperationHolder<DependencyTelemetry> operation;

            public ApplicationInsightsApmDependencyOperation(IOperationHolder<DependencyTelemetry> operation)
            {
                this.operation = operation;
            }

            public void MarkSuccess(string resultCode)
            {
                operation.Telemetry.Success = true;
                operation.Telemetry.ResultCode = resultCode;
            }

            public void MarkFailure(string resultCode, Exception exception = null)
            {
                operation.Telemetry.Success = false;
                operation.Telemetry.ResultCode = resultCode;

                if (exception != null)
                {
                    operation.Telemetry.Properties["ExceptionType"] = exception.GetType().Name;
                    operation.Telemetry.Properties["ExceptionMessage"] = exception.Message;
                }
            }
        }
    }
}