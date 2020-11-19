using System;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Apm
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that provides compile-time integration with the registered APM tool.
    /// </summary>
    public class ApmSpanFrame : SyncFrame
    {
        private readonly string _spanKind;
        private readonly string _operationName;
        private readonly string _type;

        private readonly Variable _spanVariable;

        private ApmSpanFrame(
            string spanKind,
            string operationName,
            string type)
        {
            this._spanKind = spanKind;
            this._operationName = operationName;
            this._type = type;

            this._spanVariable = new Variable(
                typeof(IApmSpan),
                "apmSpanOf" + operationName.Replace(" ", string.Empty).Replace("-", string.Empty),
                this);
        }

        /// <summary>
        /// Creates a 'start' frame, a <see cref="SyncFrame" /> that will call <see cref="IApmSpan.StartSpan" /> on the current span
        /// to create a new child.
        /// </summary>
        /// <param name="spanKind">The span kind (<see cref="SpanKinds" />).</param>
        /// <param name="operationName">The name of the operation.</param>
        /// <param name="type">The type of the operation.</param>
        /// <returns>A new <see cref="ApmSpanFrame" />.</returns>
        public static ApmSpanFrame Start(
            string spanKind,
            string operationName,
            string type)
        {
            return new ApmSpanFrame(spanKind, operationName, type);
        }

        /// <summary>
        /// Returns a <see cref="Frame" /> that will Dispose / complete the span this<see cref="ApmSpanFrame" /> has
        /// created, therefore marking the section as done and to record the time it took.
        /// </summary>
        /// <returns>A new <see cref="Frame" /> to end the span created by this <see cref="ApmSpanFrame" />.</returns>
        public Frame Complete()
        {
            return new EndApmFrame(this._spanVariable);
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            // Note that we cannot just get a variable of type IApmSpan as _we_ create one, which causes a loop and failed compilation
            var context = variables.FindVariable(typeof(ApiOperationContext));
            var currentSpan = context.GetProperty(nameof(ApiOperationContext.ApmSpan));

            writer.WriteLine($"using var {this._spanVariable} = {currentSpan}.{nameof(IApmSpan.StartSpan)}(\"{this._spanKind}\", \"{this._operationName}\", \"{this._type}\");");

            next();
        }

        private class EndApmFrame : SyncFrame
        {
            private readonly Variable _spanVariable;

            public EndApmFrame(Variable spanVariable)
            {
                this._spanVariable = spanVariable;
            }

            /// <inheritdoc />
            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                writer.WriteLine($"{this._spanVariable}.{nameof(IApmSpan.Dispose)}();");

                next();
            }
        }
    }
}
