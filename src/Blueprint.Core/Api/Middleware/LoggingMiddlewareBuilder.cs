using System;
using System.Collections.Generic;
using System.Diagnostics;

using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core.Api.CodeGen;
using Blueprint.Core.Utilities;

namespace Blueprint.Core.Api.Middleware
{
    /// <summary>
    /// A middleware that performs logging of API operation executions.
    /// </summary>
    public class LoggingMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <inheritdoc />
        /// <returns><see cref="ApiOperationDescriptor.ShouldAudit"/></returns>
        public bool Matches(ApiOperationDescriptor operation)
        {
            return operation.ShouldAudit;
        }

        /// <inheritdoc />
        public void Build(MiddlewareBuilderContext context)
        {
            var message = $"API operation finished. operation_type={context.Descriptor.OperationType.Name}";

            var methodCall = new MethodCall(typeof(StopwatchLogger), nameof(StopwatchLogger.LogTime));
            methodCall.TrySetArgument("message", new Variable(typeof(string), $"\"{message}\""));
            methodCall.TrySetArgument("args", new Variable(typeof(object[]), "System.Array.Empty<object>()"));

            context.ExecuteMethod.Frames.Add(new LoggingFrame(context.Descriptor.OperationType));
        }

        private class LoggingFrame : SyncFrame
        {
            private readonly LogFrame logFrame;

            public LoggingFrame(Type operationType)
            {
                var message = $"API operation finished. operation_type={operationType.Name} time_taken_ms={{0}}";
                logFrame = LogFrame.Info(message, "stopwatch.ElapsedMilliseconds");
            }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                writer.Write($"var stopwatch = {typeof(Stopwatch).FullNameInCode()}.StartNew();");
                writer.Write("stopwatch.Start();");

                writer.Write("BLOCK:try");
                Next?.GenerateCode(method, writer);
                writer.FinishBlock();

                writer.Write("BLOCK:finally");
                writer.Write("stopwatch.Stop();");

                logFrame.GenerateCode(method, writer);

                writer.FinishBlock();
            }

            public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
            {
                return logFrame.FindVariables(chain);
            }
        }
    }
}
