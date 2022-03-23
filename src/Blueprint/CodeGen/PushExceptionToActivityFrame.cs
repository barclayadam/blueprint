using System;
using System.Diagnostics;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Diagnostics;
using OpenTelemetry.Trace;

namespace Blueprint.CodeGen
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that can be added to exception handling blocks to push the
    /// exception to the <see cref="Activity" /> of the <see cref="ApiOperationContext" />.
    /// </summary>
    public class PushExceptionToActivityFrame : SyncFrame
    {
        private readonly Variable _exceptionVariable;
        private readonly bool _escaped;

        /// <summary>
        /// Initialises a new instance of the <see cref="PushExceptionToActivityFrame" /> class.
        /// </summary>
        /// <param name="exceptionVariable">The <see cref="Variable" /> that represents the thrown exception.</param>
        /// <param name="escaped">Whether the exception event is recorded at a point where it is known that the exception is escaping the scope of the span. </param>
        public PushExceptionToActivityFrame(Variable exceptionVariable, bool escaped = true)
        {
            this._exceptionVariable = exceptionVariable;
            this._escaped = escaped;
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            var context = variables.FindVariable(typeof(ApiOperationContext));
            var activityVariable = context.GetProperty(nameof(ApiOperationContext.Activity));

            writer.WriteLine($"{typeof(BlueprintActivitySource).FullNameInCode()}.{nameof(BlueprintActivitySource.RecordException)}({activityVariable}, {this._exceptionVariable}, {this._escaped.ToString().ToLowerInvariant()});");

            writer.If($"{this._exceptionVariable} is {typeof(ApiException).FullNameInCode()} apiException && apiException.{nameof(ApiException.HttpStatus)} < 500");
            writer.Write(new ActivityStatusFrame(activityVariable, Status.Ok));
            writer.FinishBlock();

            writer.Else();
            writer.Write(new ActivityStatusFrame(activityVariable, Status.Error));
            writer.FinishBlock();
        }
    }
}
