using System;
using Blueprint.Api;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core.Authorisation;
using Datadog.Trace;

namespace Blueprint.Apm.DataDog
{
    /// <summary>
    /// A middleware component that integrates Blueprint pipeline execution with DataDog APM.
    /// </summary>
    public class DataDogApmMiddleware : IMiddlewareBuilder
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
            var startFrame = new StartFrame(context);

            context.ExecuteMethod.Frames.Insert(0, startFrame);

            context.RegisterFinallyFrames(new FinallyFrame(startFrame.ScopeOwnedVariable));

            context.RegisterUnhandledExceptionHandler(typeof(Exception), (e) =>
            {
                return new Frame[] { new CaptureExceptionFrame(e),  };
            });
        }

        private class StartFrame : SyncFrame
        {
            private readonly MiddlewareBuilderContext builderContext;

            private readonly Variable scopeVariable;
            private readonly Variable spanVariable;
            private readonly Variable scopeOwnedVariable;

            public StartFrame(MiddlewareBuilderContext builderContext)
            {
                this.builderContext = builderContext;

                this.scopeVariable = new Variable(typeof(Scope), this);
                this.spanVariable = new Variable(typeof(Span), this);

                this.scopeOwnedVariable = new Variable(typeof(bool), "scopeIsOwned", this);
            }

            public Variable ScopeOwnedVariable => scopeOwnedVariable;

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var tracerVariable = new Variable(typeof(Tracer), $"{typeof(Tracer).FullNameInCode()}.{nameof(Tracer.Instance)}");
                var operationName = builderContext.Descriptor.Name;

                writer.Write($"var {scopeOwnedVariable} = {tracerVariable}.{nameof(Tracer.ActiveScope)} == null;");
                writer.Write($"var {scopeVariable} = {scopeOwnedVariable} == false ? {tracerVariable}.{nameof(Tracer.ActiveScope)} : {tracerVariable}.{nameof(Tracer.StartActive)}(\"operation.process\");");
                writer.Write($"var {spanVariable} = {scopeVariable}.{nameof(Scope.Span)};");

                // The component is used to identify the individual "resource" / "component", which here is defined as the
                // operation name (for example that would mean a web request may be represented as operation "http.request", with a component
                // of "GetCurrentUser".
                writer.Write($"{spanVariable}.{nameof(Span.ResourceName)} = \"{operationName}\";");
                next();
            }
        }

        private class FinallyFrame : SyncFrame
        {
            private readonly Variable scopeOwnedVariable;

            public FinallyFrame(Variable scopeOwnedVariable)
            {
                this.scopeOwnedVariable = scopeOwnedVariable;
            }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var spanVariable = variables.FindVariable(typeof(Span));

                var apiOperationContextVariable = variables.FindVariable(typeof(ApiOperationContext));

                // ALWAYS, in a finally statement, try to set the user details if we have them available
                // This is so the UserAuthorisationContext variable isn't reordered above the try of this middleware.
                writer.Write($"var userContext = {apiOperationContextVariable}.{nameof(ApiOperationContext.UserAuthorisationContext)};");

                writer.WriteIf($"userContext != null && userContext.{nameof(IUserAuthorisationContext.IsAnonymous)} == false");
                writer.Write($"{spanVariable}.SetTag(\"user.id\", userContext.{nameof(IUserAuthorisationContext.Id)});");
                writer.FinishBlock();

                // Only if we created the transaction and therefore "own" it will we manually end it. If the transaction already existed (i.e. ASP.NET
                // Core integration setup in application) then we should not be ending the transaction ourselves
                writer.WriteIf($"{scopeOwnedVariable}");
                writer.Write($"{spanVariable}.{nameof(Span.Finish)}();");
                writer.FinishBlock();
            }
        }

        private class CaptureExceptionFrame : SyncFrame
        {
            private readonly Variable exceptionVariable;

            public CaptureExceptionFrame(Variable exceptionVariable)
            {
                this.exceptionVariable = exceptionVariable;
            }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var spanVariable = variables.FindVariable(typeof(Span));

                writer.Write($"{spanVariable}.{nameof(Span.SetException)}({exceptionVariable});");
            }
        }
    }
}
