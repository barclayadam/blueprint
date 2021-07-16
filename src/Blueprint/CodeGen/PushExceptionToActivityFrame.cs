using System;
using System.Diagnostics;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Diagnostics;

namespace Blueprint.CodeGen
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that can be added to exception handling blocks to push the
    /// exception to the <see cref="Activity" /> of the <see cref="ApiOperationContext" />.
    /// </summary>
    public class PushExceptionToActivityFrame : SyncFrame
    {
        private readonly Variable _exceptionVariable;

        /// <summary>
        /// Initialises a new instance of the <see cref="PushExceptionToActivityFrame" /> class.
        /// </summary>
        /// <param name="exceptionVariable">The <see cref="Variable" /> that represents the thrown exception.</param>
        public PushExceptionToActivityFrame(Variable exceptionVariable)
        {
            this._exceptionVariable = exceptionVariable;
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            var context = variables.FindVariable(typeof(ApiOperationContext));
            var activityVariable = context.GetProperty(nameof(ApiOperationContext.Activity));

            writer.WriteLine($"{typeof(BlueprintActivitySource).FullNameInCode()}.{nameof(BlueprintActivitySource.RecordException)}({activityVariable}, {this._exceptionVariable});");
        }
    }
}
