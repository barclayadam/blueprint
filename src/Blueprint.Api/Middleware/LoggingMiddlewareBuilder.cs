using System;
using System.Collections.Generic;
using System.Diagnostics;
using Blueprint.Api.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Microsoft.Extensions.Logging;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// A middleware that performs logging of API operation executions.
    /// </summary>
    public class LoggingMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <inheritdoc />
        /// <returns><see cref="ApiOperationDescriptor.ShouldAudit"/>.</returns>
        public bool Matches(ApiOperationDescriptor operation)
        {
            return operation.ShouldAudit;
        }

        /// <inheritdoc />
        public void Build(MiddlewareBuilderContext context)
        {
            context.ExecuteMethod.Frames.Add(new LoggingStartFrame());
            context.RegisterFinallyFrames(new LoggingEndFrame(context.Descriptor.OperationType));
        }

        private class LoggingStartFrame : SyncFrame
        {
            private readonly Variable stopwatchVariable;

            public LoggingStartFrame()
            {
                stopwatchVariable = new Variable(typeof(Stopwatch), this);
            }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                writer.Write($"var {stopwatchVariable} = {typeof(Stopwatch).FullNameInCode()}.{nameof(Stopwatch.StartNew)}();");

                Next?.GenerateCode(method, writer);
            }
        }

        private class LoggingEndFrame : SyncFrame
        {
            private readonly Type operationType;
            private Variable stopwatchVariable;
            private Variable loggerVariable;

            public LoggingEndFrame(Type operationType)
            {
                this.operationType = operationType;
            }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                writer.Write($"{stopwatchVariable}.Stop();");

                LogFrame.Information(
                    method,
                    writer,
                    loggerVariable,
                    "Operation {0} finished in {1}ms",
                    $"\"{operationType}\"",
                    $"{stopwatchVariable}.{nameof(Stopwatch.Elapsed)}.{nameof(TimeSpan.TotalMilliseconds)}");
            }

            public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
            {
                yield return stopwatchVariable = chain.FindVariable(typeof(Stopwatch));
                yield return loggerVariable = chain.FindVariable(typeof(ILogger));
            }
        }
    }
}
