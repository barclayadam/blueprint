using System;
using Blueprint.Api;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core.Authorisation;
using Elastic.Apm;
using Elastic.Apm.Api;

namespace Blueprint.Apm.Elastic
{
    /// <summary>
    /// A middleware component that will create / modify <see cref="ITransaction" />s to integrate
    /// with Elastic APM.
    /// </summary>
    public class ElasticApmMiddleware : IMiddlewareBuilder
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

            context.RegisterFinallyFrames(new FinallyFrame(startFrame.TransactionOwnedVariable));

            context.RegisterUnhandledExceptionHandler(typeof(Exception), (e) =>
            {
                return new Frame[] { new CaptureExceptionFrame(e),  };
            });
        }

        private class StartFrame : SyncFrame
        {
            private readonly MiddlewareBuilderContext builderContext;

            private readonly Variable transactionVariable;
            private readonly Variable transactionOwnedVariable;

            public StartFrame(MiddlewareBuilderContext builderContext)
            {
                this.builderContext = builderContext;
                this.transactionVariable = new Variable(typeof(ITransaction), this);
                this.transactionOwnedVariable = new Variable(typeof(bool), this);
            }

            public Variable TransactionOwnedVariable => transactionOwnedVariable;

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var operationName = builderContext.Descriptor.Name;
                var tracer = $"{typeof(Agent).FullNameInCode()}.{nameof(Agent.Tracer)}";
                var currentTransaction = $"{tracer}.{nameof(ITracer.CurrentTransaction)}";

                writer.Write($"var {transactionOwnedVariable} = {currentTransaction} == null;");
                writer.Write($"var {transactionVariable} = {currentTransaction} ?? ");
                writer.Write($"   {tracer}.{nameof(Agent.Tracer.StartTransaction)}(\"{operationName}\", \"Operation\");");

                // Always set the Name of the transaction, to modify existing transactions to have a better, more accurate name
                writer.Write($"{transactionVariable}.{nameof(ITransaction.Name)} = \"{operationName}\";");

                next();
            }
        }

        private class FinallyFrame : SyncFrame
        {
            private readonly Variable transactionOwnedVariable;

            public FinallyFrame(Variable transactionOwnedVariable)
            {
                this.transactionOwnedVariable = transactionOwnedVariable;
            }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var transactionVariable = variables.FindVariable(typeof(ITransaction));
                var apiOperationContextVariable = variables.FindVariable(typeof(ApiOperationContext));

                // ALWAYS, in a finally statement, try to set the user details if we have them available
                // This is so the UserAuthorisationContext variable isn't reordered above the try of this middleware.
                writer.Write($"var userContext = {apiOperationContextVariable}.{nameof(ApiOperationContext.UserAuthorisationContext)};");

                writer.WriteIf($"userContext != null && userContext.{nameof(IUserAuthorisationContext.IsAnonymous)} == false");
                writer.Write($"{transactionVariable}.Context.User = new {typeof(User).FullNameInCode()} {{ Id = userContext.{nameof(IUserAuthorisationContext.Id)} }};");
                writer.FinishBlock();

                // Only if we created the transaction and therefore "own" it will we manually end it. If the transaction already existed (i.e. ASP.NET
                // Core integration setup in application) then we should not be ending the transaction ourselves
                writer.WriteIf($"{transactionOwnedVariable}");
                writer.Write($"{transactionVariable}.{nameof(ITransaction.End)}();");
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
                var requestTelemetryVariable = variables.FindVariable(typeof(ITransaction));

                // Set explicitly to true unless it has been previously set to false
                writer.Write($"{requestTelemetryVariable}.{nameof(ITransaction.CaptureException)}({exceptionVariable});");
            }
        }
    }
}
