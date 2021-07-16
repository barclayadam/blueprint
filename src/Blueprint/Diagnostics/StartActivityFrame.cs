using System;
using System.Diagnostics;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Diagnostics
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that provides compile-time integration with the registered APM tool.
    /// </summary>
    public class StartActivityFrame : SyncFrame
    {
        private readonly ActivityKind _spanKind;
        private readonly string _operationName;

        private readonly Variable _activityVariable;

        private StartActivityFrame(
            ActivityKind spanKind,
            string operationName)
        {
            this._spanKind = spanKind;
            this._operationName = operationName;

            this._activityVariable = new Variable(
                typeof(Activity),
                "activityOf" + operationName.Replace(" ", string.Empty).Replace("-", string.Empty).Replace("`", string.Empty),
                this);
        }

        /// <summary>
        /// Creates a 'start' frame, a <see cref="SyncFrame" /> that will call <see cref="ActivitySource.StartActivity(string,System.Diagnostics.ActivityKind)" /> on the
        /// Blueprint activity source.
        /// </summary>
        /// <param name="activityKind">The span kind (<see cref="ActivityKind" />).</param>
        /// <param name="operationName">The name of the operation.</param>
        /// <returns>A new <see cref="StartActivityFrame" />.</returns>
        public static StartActivityFrame Start(
            ActivityKind activityKind,
            string operationName)
        {
            return new (activityKind, operationName);
        }

        /// <summary>
        /// Returns a <see cref="Frame" /> that will Dispose / complete the span this<see cref="StartActivityFrame" /> has
        /// created, therefore marking the section as done and to record the time it took.
        /// </summary>
        /// <returns>A new <see cref="Frame" /> to end the span created by this <see cref="StartActivityFrame" />.</returns>
        public Frame Complete()
        {
            return new EndApmFrame(this._activityVariable);
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            writer.WriteLine($"using var {this._activityVariable} = {typeof(BlueprintActivitySource).FullNameInCode()}.{nameof(BlueprintActivitySource.ActivitySource)}.{nameof(ActivitySource.StartActivity)}(\"{this._operationName}\", {typeof(ActivityKind).FullNameInCode()}.{this._spanKind});");

            next();
        }

        private class EndApmFrame : SyncFrame
        {
            private readonly Variable _activityVariable;

            public EndApmFrame(Variable activityVariable)
            {
                this._activityVariable = activityVariable;
            }

            /// <inheritdoc />
            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                writer.WriteLine($"{this._activityVariable}?.{nameof(Activity.Dispose)}();");

                next();
            }
        }
    }
}
