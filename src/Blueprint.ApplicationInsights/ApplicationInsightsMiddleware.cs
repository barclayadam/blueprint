using System;
using Blueprint.Api;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core.Authorisation;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;

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
            context.ExecuteMethod.Sources.Add(new RequestTelemetrySource());

            context.ExecuteMethod.Frames.Add(new LoggingStartFrame(context));

            context.RegisterFinallyFrames(new LoggingEndFrame());
        }

        private class LoggingStartFrame : SyncFrame
        {
            private readonly MiddlewareBuilderContext builderContext;

            public LoggingStartFrame(MiddlewareBuilderContext builderContext)
            {
                this.builderContext = builderContext;
            }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var requestTelemetryVariable = variables.FindVariable(typeof(RequestTelemetry));
                var operationName = builderContext.Descriptor.HttpMethod + " " + builderContext.Descriptor.OperationType.Name;

                // Must check if requestTelemetry actually exists. Set the operation name to that of the HTTP method + operation class name
                writer.WriteIf($"{requestTelemetryVariable} != null");
                writer.Write($"{requestTelemetryVariable}.Name = \"{operationName}\";");
                writer.FinishBlock();

                next();
            }
        }

        private class LoggingEndFrame : SyncFrame
        {
            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var requestTelemetryVariable = variables.FindVariable(typeof(RequestTelemetry));
                var apiOperationContextVariable = variables.FindVariable(typeof(ApiOperationContext));

                // ALWAYS, in a finally statement, try to set the user details if we have them available
                // This is so the UserAuthorisationContext variable isn't reordered above the try of this middleware.
                writer.Write($"var userContext = {apiOperationContextVariable}.{nameof(ApiOperationContext.UserAuthorisationContext)};");
                writer.WriteIf($"{requestTelemetryVariable} != null && userContext != null && userContext.{nameof(IUserAuthorisationContext.IsAnonymous)} == false");
                writer.Write($"{requestTelemetryVariable}.Context.User.AuthenticatedUserId = userContext.{nameof(IUserAuthorisationContext.Id)};");
                writer.Write($"{requestTelemetryVariable}.Context.User.AccountId = userContext.{nameof(IUserAuthorisationContext.AccountId)};");
                writer.FinishBlock();
            }
        }

        private class RequestTelemetrySource : IVariableSource
        {
            public Variable TryFindVariable(IMethodVariables variables, Type type)
            {
                if (type == typeof(RequestTelemetry))
                {
                    var httpContextVariable = variables.FindVariable(typeof(HttpContext));

                    return new VariableSourceFrame(
                        type,
                        $"{httpContextVariable}.{nameof(HttpContext.Features)}.{nameof(HttpContext.Features.Get)}<{typeof(RequestTelemetry).FullNameInCode()}>()")
                        .SourceVariable;
                }

                return null;
            }
        }

        private class VariableSourceFrame : SyncFrame
        {
            private readonly Variable sourceVariable;
            private readonly string use;

            public VariableSourceFrame(Type type, string use)
            {
                sourceVariable = new Variable(type, this);

                this.use = use;
            }

            public Variable SourceVariable => sourceVariable;

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                writer.Write($"var {sourceVariable} = {use};");

                next();
            }
        }
    }
}
