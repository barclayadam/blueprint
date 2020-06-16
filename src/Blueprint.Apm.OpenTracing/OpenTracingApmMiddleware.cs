using System;
using System.Collections.Generic;
using Blueprint.Api;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core.Authorisation;
using OpenTracing;

namespace Blueprint.Apm.OpenTracing
{
    /// <summary>
    /// A middleware component that create OpenTracing <see cref="ISpan" />s to identify the
    /// </summary>
    public class OpenTracingApmMiddleware : IMiddlewareBuilder
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

            context.RegisterFinallyFrames(new FinallyFrame(startFrame.SpanOwnedVariable));

            context.RegisterUnhandledExceptionHandler(typeof(Exception), (e) =>
            {
                return new Frame[] { new CaptureExceptionFrame(e),  };
            });
        }

        private class StartFrame : SyncFrame
        {
            private readonly MiddlewareBuilderContext builderContext;

            private readonly Variable spanVariable;
            private readonly Variable spanOwnedVariable;

            public StartFrame(MiddlewareBuilderContext builderContext)
            {
                this.builderContext = builderContext;

                this.spanVariable = new Variable(typeof(ISpan), this);
                this.spanOwnedVariable = new Variable(typeof(bool), this);
            }

            public Variable SpanOwnedVariable => spanOwnedVariable;

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var tracerVariable = variables.FindVariable(typeof(ITracer));
                var operationName = builderContext.Descriptor.Name;

                writer.Write($"var {spanOwnedVariable} = {tracerVariable}.{nameof(ITracer.ActiveSpan)} == null;");
                writer.Write($"var {spanVariable} = {spanOwnedVariable} == false ? {tracerVariable}.{nameof(ITracer.ActiveSpan)} : {tracerVariable}.{nameof(ITracer.BuildSpan)}(\"{operationName}\").StartActive().Span;");

                // Always set the Name of the transaction, to modify existing transactions to have a better, more accurate name
                writer.Write($"{spanVariable}.{nameof(ISpan.SetOperationName)}(\"{operationName}\");");
                next();
            }
        }

        private class FinallyFrame : SyncFrame
        {
            private readonly Variable spanOwnedVariable;

            public FinallyFrame(Variable spanOwnedVariable)
            {
                this.spanOwnedVariable = spanOwnedVariable;
            }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var spanVariable = variables.FindVariable(typeof(ISpan));

                var apiOperationContextVariable = variables.FindVariable(typeof(ApiOperationContext));

                // ALWAYS, in a finally statement, try to set the user details if we have them available
                // This is so the UserAuthorisationContext variable isn't reordered above the try of this middleware.
                writer.Write($"var userContext = {apiOperationContextVariable}.{nameof(ApiOperationContext.UserAuthorisationContext)};");

                writer.WriteIf($"userContext != null && userContext.{nameof(IUserAuthorisationContext.IsAnonymous)} == false");
                writer.Write($"{spanVariable}.SetTag(\"user.id\", userContext.{nameof(IUserAuthorisationContext.Id)});");
                writer.FinishBlock();

                // Only if we created the transaction and therefore "own" it will we manually end it. If the transaction already existed (i.e. ASP.NET
                // Core integration setup in application) then we should not be ending the transaction ourselves
                writer.WriteIf($"{spanOwnedVariable}");
                writer.Write($"{spanVariable}.{nameof(ISpan.Finish)}();");
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
                var spanVariable = variables.FindVariable(typeof(ISpan));

                writer.Write($"{spanVariable}.SetTag(\"error\", true);");

                writer.Write($"{spanVariable}.Log(new {typeof(Dictionary<string, object>).FullNameInCode()} {{");
                writer.Write($"    [\"event\"] = \"error\",");
                writer.Write($"    [\"message\"] = {exceptionVariable}.Message,");
                writer.Write($"    [\"error.kind\"] = \"Exception\",");
                writer.Write($"    [\"error.object\"] = {exceptionVariable},");
                writer.Write($"    [\"error.stack\"] = {exceptionVariable}.StackTrace,");
                writer.Write("});");
            }
        }
    }
}
