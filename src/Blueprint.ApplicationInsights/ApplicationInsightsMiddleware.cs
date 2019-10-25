using System.Collections.Generic;
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
        /// Initialises a new instance of the <see cref="ApplicationInsightsMiddleware" /> middleware builder.
        /// </summary>
        public ApplicationInsightsMiddleware()
        {
        }

        /// <inheritdoc />
        /// <returns><c>true</c>.</returns>
        public bool Matches(ApiOperationDescriptor operation)
        {
            return true;
        }

        /// <inheritdoc />
        public void Build(MiddlewareBuilderContext context)
        {
            context.ExecuteMethod.Frames.Add(new LoggingStartFrame(context));
            context.RegisterFinallyFrames(new LoggingEndFrame());
        }

        private class LoggingStartFrame : SyncFrame
        {
            private readonly MiddlewareBuilderContext builderContext;
            private readonly Variable requestTelemetryVariable;

            private Variable httpContextVariable;

            public LoggingStartFrame(MiddlewareBuilderContext builderContext)
            {
                this.builderContext = builderContext;
                requestTelemetryVariable = new Variable(typeof(RequestTelemetry), this);
            }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                var operationName = builderContext.Descriptor.HttpMethod + " " + builderContext.Descriptor.OperationType.Name;

                writer.Write($"var {requestTelemetryVariable} = {httpContextVariable}.{nameof(HttpContext.Features)}.Get<{typeof(RequestTelemetry).FullNameInCode()}>();");
                writer.BlankLine();

                // Must check if requestTelemetry actually exists. Set the operation name to that of the HTTP method + operation class name
                writer.WriteIf($"{requestTelemetryVariable} != null");
                writer.Write($"{requestTelemetryVariable}.Name = \"{operationName}\";");
                writer.FinishBlock();

                Next?.GenerateCode(method, writer);
            }

            /// <inheritdoc />
            public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
            {
                yield return httpContextVariable = chain.FindVariable(typeof(HttpContext));
            }
        }

        private class LoggingEndFrame : SyncFrame
        {
            private Variable requestTelemetryVariable;
            private Variable apiOperationContextVariable;

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                // ALWAYS, in a finally statement, try to set the user details if we have them available
                // This is so the UserAuthorisationContext variable isn't reordered above the try of this middleware.
                writer.Write($"var userContext = {apiOperationContextVariable}.{nameof(ApiOperationContext.UserAuthorisationContext)};");
                writer.WriteIf($"{requestTelemetryVariable} != null && userContext != null && userContext.{nameof(IUserAuthorisationContext.IsAnonymous)} == false");
                writer.Write($"{requestTelemetryVariable}.Context.User.AuthenticatedUserId = userContext.{nameof(IUserAuthorisationContext.Id)};");
                writer.Write($"{requestTelemetryVariable}.Context.User.AccountId = userContext.{nameof(IUserAuthorisationContext.AccountId)};");
                writer.FinishBlock();
            }

            public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
            {
                yield return requestTelemetryVariable = chain.FindVariable(typeof(RequestTelemetry));
                yield return apiOperationContextVariable = chain.FindVariable(typeof(ApiOperationContext));
            }
        }
    }
}
