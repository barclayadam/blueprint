using System;
using System.Collections.Generic;
using System.Diagnostics;
using Blueprint.Api.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

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
            private readonly LogFrame logFrame;
            private Variable stopwatchVariable;

            public LoggingEndFrame(Type operationType)
            {
                var message = $"Operation {{0}} finished in {{1}}ms";
                logFrame = LogFrame.Information(message, $"\"{operationType}\"", "stopwatch.ElapsedMilliseconds");
            }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                writer.Write($"{stopwatchVariable}.Stop();");
                logFrame.GenerateCode(method, writer);
            }

            public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
            {
                yield return stopwatchVariable = chain.FindVariable(typeof(Stopwatch));

                foreach (var v in logFrame.FindVariables(chain))
                {
                    yield return v;
                }
            }
        }
    }
}
