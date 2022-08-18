using System;
using System.Diagnostics;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using JetBrains.Annotations;
using OpenTelemetry.Trace;

namespace Blueprint.Diagnostics
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that writes a status code to an <see cref="Activity" />.
    /// </summary>
    public class ActivityStatusFrame : SyncFrame
    {
        private readonly Variable _activityVariable;
        private readonly StatusCode _status;
        [CanBeNull]
        private readonly Variable _statusDescription;

        /// <summary>
        /// Initialises a new instance of the <see cref="ActivityStatusFrame"/> class.
        /// </summary>
        /// <param name="activityVariable">The variable containing the <see cref="Activity" /> to write a status to.</param>
        /// <param name="status">The status code to set.</param>
        public ActivityStatusFrame(
            Variable activityVariable,
            StatusCode status)
        {
            this._activityVariable = activityVariable;
            this._status = status;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ActivityStatusFrame"/> class.
        /// </summary>
        /// <param name="activityVariable">The variable containing the <see cref="Activity" /> to write a status to.</param>
        /// <param name="status">The status code to set.</param>
        /// <param name="statusDescription">The description of this status, for example an exception message.</param>
        public ActivityStatusFrame(
            Variable activityVariable,
            StatusCode status,
            Variable statusDescription)
        {
            this._activityVariable = activityVariable;
            this._status = status;
            this._statusDescription = statusDescription;
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            writer.WriteLine($"{this._activityVariable}?.SetTag(\"otel.status_code\", \"{this._status.ToString().ToUpperInvariant()}\");");

            if (this._statusDescription != null)
            {
                writer.WriteLine($"{this._activityVariable}?.SetTag(\"otel.status_description\", {this._statusDescription});");
            }

            next();
        }
    }
}
