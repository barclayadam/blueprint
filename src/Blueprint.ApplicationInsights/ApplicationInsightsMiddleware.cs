using System;
using Blueprint.Api;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core.Authorisation;
using Microsoft.ApplicationInsights.DataContracts;

namespace Blueprint.ApplicationInsights
{
    /// <summary>
    /// A middleware component that will set data on the current <see cref="RequestTelemetry" /> that
    /// ApplicationInsights has created.
    /// </summary>
    /// <remarks>
    /// This middleware is optional but provides better naming (by using the operation type instead of
    /// the default full name), and will set AuthenticatedUserId and AccountId for the current user.
    /// </remarks>
    public class ApplicationInsightsMiddleware : IMiddlewareBuilder
    {
        /// <summary>
        /// Returns <c>false</c>.
        /// </summary>
        public bool SupportsNestedExecution => false;

        /// <inheritdoc />
        /// <returns><c>true</c>.</returns>
        public bool Matches(ApiOperationDescriptor operation)
        {
            return true;
        }

        /// <inheritdoc />
        public void Build(MiddlewareBuilderContext context)
        {
            context.ExecuteMethod.Frames.Add(new StartFrame(context));

            context.RegisterFinallyFrames(new EndFrame());

            context.RegisterUnhandledExceptionHandler(typeof(Exception), (e) =>
            {
                return new Frame[] { new SetSuccessFrame(false) };
            });
        }

        private class StartFrame : SyncFrame
        {
            private readonly MiddlewareBuilderContext builderContext;

            public StartFrame(MiddlewareBuilderContext builderContext)
            {
                this.builderContext = builderContext;
            }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var requestTelemetryVariable = variables.FindVariable(typeof(RequestTelemetry));
                var operationName = builderContext.Descriptor.Name;

                // Must check if requestTelemetry actually exists. Set the operation name to that of the HTTP method + operation class name
                writer.WriteIf($"{requestTelemetryVariable} != null");
                writer.Write($"{requestTelemetryVariable}.Name = \"{operationName}\";");
                writer.FinishBlock();

                next();
            }
        }

        private class EndFrame : SyncFrame
        {
            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var requestTelemetryVariable = variables.FindVariable(typeof(RequestTelemetry));
                var apiOperationContextVariable = variables.FindVariable(typeof(ApiOperationContext));

                // ALWAYS, in a finally statement, try to set the user details if we have them available
                // This is so the UserAuthorisationContext variable isn't reordered above the try of this middleware.
                writer.Write($"var userContext = {apiOperationContextVariable}.{nameof(ApiOperationContext.UserAuthorisationContext)};");

                writer.WriteIf($"{requestTelemetryVariable} != null");

                writer.WriteIf($"userContext != null && userContext.{nameof(IUserAuthorisationContext.IsAnonymous)} == false");
                writer.Write($"{requestTelemetryVariable}.Context.User.AuthenticatedUserId = userContext.{nameof(IUserAuthorisationContext.Id)};");
                writer.Write($"{requestTelemetryVariable}.Context.User.AccountId = userContext.{nameof(IUserAuthorisationContext.AccountId)};");

                writer.FinishBlock();

                // Set explicitly to true unless it has been previously set to false
                writer.Write($"{requestTelemetryVariable}.Success = {requestTelemetryVariable}.Success ?? true;");

                writer.FinishBlock();
            }
        }

        private class SetSuccessFrame : SyncFrame
        {
            private readonly bool value;

            public SetSuccessFrame(bool value)
            {
                this.value = value;
            }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var requestTelemetryVariable = variables.FindVariable(typeof(RequestTelemetry));

                writer.WriteIf($"{requestTelemetryVariable} != null");

                // Set explicitly to true unless it has been previously set to false
                writer.Write($"{requestTelemetryVariable}.Success = {value.ToString().ToLowerInvariant()};");

                writer.FinishBlock();

            }
        }
    }
}
