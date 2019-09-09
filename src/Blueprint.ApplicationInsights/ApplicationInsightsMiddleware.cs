using System.Collections.Generic;
using Blueprint.Api;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core;
using Blueprint.Core.Security;
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
        /// <inheritdoc />
        /// <returns><c>true</c></returns>
        public bool Matches(ApiOperationDescriptor operation)
        {
            return true;
        }

        /// <inheritdoc />
        public void Build(MiddlewareBuilderContext context)
        {
            context.ExecuteMethod.Frames.Append(new ApplicationInsightsFrame(context));
        }

        private class ApplicationInsightsFrame : SyncFrame
        {
            private readonly MiddlewareBuilderContext context;

            private Variable httpContextVariable;
            private Variable apiOperationContextVariable;

            public ApplicationInsightsFrame(MiddlewareBuilderContext context)
            {
                this.context = context;
            }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                var operationName = context.Descriptor.HttpMethod + " " + context.Descriptor.OperationType.Name;

                writer.Write($"var requestTelemetry = {httpContextVariable}.{nameof(HttpContext.Features)}.Get<{typeof(RequestTelemetry).FullNameInCode()}>();");
                writer.BlankLine();

                // Must check if requestTelemetry actually exists. Set the operation name to that of the HTTP method + operation class name
                writer.WriteIf("requestTelemetry != null");
                writer.Write($"requestTelemetry.Name = \"{operationName}\";");
                writer.FinishBlock();

                writer.WriteTry();
                Next?.GenerateCode(method, writer);

                writer.WriteFinally();
                // ALWAYS, in a finally statement, try to set the user details if we have them available
                // This is so the UserAuthorisationContext variable isn't reordered above the try of this middleware
                writer.Write($"var userContext = {apiOperationContextVariable}.{nameof(ApiOperationContext.UserAuthorisationContext)};");
                writer.WriteIf($"requestTelemetry != null && userContext != null && userContext.{nameof(IUserAuthorisationContext.IsAnonymous)} == false");
                writer.Write($"requestTelemetry.Context.User.AuthenticatedUserId = userContext.{nameof(IUserAuthorisationContext.Id)};");
                writer.Write($"requestTelemetry.Context.User.AccountId = userContext.{nameof(IUserAuthorisationContext.AccountId)};");
                writer.FinishBlock();

                writer.FinishBlock();
            }

            public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
            {
                httpContextVariable = chain.FindVariable(typeof(HttpContext));
                apiOperationContextVariable = chain.FindVariable(typeof(ApiOperationContext));

                yield return httpContextVariable;
                yield return apiOperationContextVariable;
            }
        }
    }
}
