using System;
using Blueprint.Apm;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.CodeGen
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that can be added to exception handling blocks to push the
    /// exception to the active span of the operation.
    /// </summary>
    public class PushExceptionToApmSpanFrame : SyncFrame
    {
        private readonly Variable exceptionVariable;

        /// <summary>
        /// Initialises a new instance of the <see cref="PushExceptionToApmSpanFrame" /> class.
        /// </summary>
        /// <param name="exceptionVariable">The <see cref="Variable" /> that represents the thrown exception.</param>
        public PushExceptionToApmSpanFrame(Variable exceptionVariable)
        {
            this.exceptionVariable = exceptionVariable;
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            var spanVariable = variables.FindVariable(typeof(IApmSpan));

            writer.Write($"{spanVariable}?.{nameof(IApmSpan.RecordException)}({exceptionVariable});");
        }
    }
}
