using System;
using System.Diagnostics;
using Blueprint.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;
using Blueprint.Utilities;
using Microsoft.Extensions.Logging;

namespace Blueprint.Middleware;

/// <summary>
/// A middleware that performs logging of API operation executions.
/// </summary>
public class LoggingMiddlewareBuilder : IMiddlewareBuilder
{
    private static readonly EventId _operationFinishedLogEvent = new EventId(4, "OperationFinished");

    /// <summary>
    /// Returns <c>true</c>.
    /// </summary>
    public bool SupportsNestedExecution => true;

    /// <inheritdoc />
    /// <returns><see cref="ApiOperationDescriptor.ShouldAudit"/>.</returns>
    public bool Matches(ApiOperationDescriptor operation)
    {
        return operation.ShouldAudit;
    }

    /// <inheritdoc />
    public void Build(MiddlewareBuilderContext context)
    {
        // Force the creation of the stopwatch variable to be at the very start of the method, ensures it
        // starts before _anything_ happens and makes it available to the finally block too
        context.ExecuteMethod.Frames.Insert(0, new LoggingStartFrame());
        context.RegisterFinallyFrames(new LoggingEndFrame(context.Descriptor.OperationType));
    }

    private class LoggingStartFrame : SyncFrame
    {
        private readonly Variable _stopwatchVariable;

        public LoggingStartFrame()
        {
            this._stopwatchVariable = new Variable(typeof(Stopwatch), this);
        }

        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            writer.WriteLine($"var {this._stopwatchVariable} = {typeof(Stopwatch).FullNameInCode()}.{nameof(Stopwatch.StartNew)}();");

            next();
        }
    }

    private class LoggingEndFrame : SyncFrame
    {
        private readonly Type _operationType;

        public LoggingEndFrame(Type operationType)
        {
            this._operationType = operationType;
        }

        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            var stopwatchVariable = variables.FindVariable(typeof(Stopwatch));

            writer.WriteLine($"{stopwatchVariable}.Stop();");

            writer.Write(
                LogFrame.Information(
                    _operationFinishedLogEvent,
                    "Operation {OperationName} finished in {TotalMilliseconds}ms",
                    ReflectionUtilities.PrettyTypeName(this._operationType),
                    stopwatchVariable.GetProperty(nameof(Stopwatch.Elapsed)).GetProperty(nameof(TimeSpan.TotalMilliseconds))));
        }
    }
}