using System;
using System.Diagnostics;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using OpenTelemetry.Trace;

namespace Blueprint.Diagnostics
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that writes a status code to an <see cref="Activity" />.
    /// </summary>
    public class ActivityStatusFrame : SyncFrame
    {
        private readonly Variable _activityVariable;
        private readonly Variable _statusVariableVariable;

        /// <summary>
        /// Initialises a new instance of the <see cref="ActivityStatusFrame"/> class.
        /// </summary>
        /// <param name="activityVariable">The variable containing the <see cref="Activity" /> to write a status to.</param>
        /// <param name="status">The status code to set.</param>
        public ActivityStatusFrame(
            Variable activityVariable,
            Status status)
        {
            this._activityVariable = activityVariable;
            this._statusVariableVariable = new Variable(typeof(Status), $"{typeof(Status).FullNameInCode()}.{status.StatusCode.ToString()}");
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            writer.If($"{this._activityVariable} != null");
            writer.WriteLine($"{typeof(ActivityExtensions).FullNameInCode()}.{nameof(ActivityExtensions.SetStatus)}({this._activityVariable}, {this._statusVariableVariable});");
            writer.FinishBlock();

            next();
        }
    }
}
