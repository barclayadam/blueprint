using System;
using System.Collections.Generic;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Microsoft.Extensions.Logging;

namespace Blueprint.Api.CodeGen
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that will output a log message to the configured logger (<see cref="ILogger"/>)
    /// for the operation at the specified level.
    /// </summary>
    /// <remarks>
    /// The logger used in output code is found by searching for a configured <see cref="LoggerVariable"/>
    /// and will be used to check whether the log level is enabled, <b>not</b> outputting any logging code in the case of
    /// the log level being turned off.
    /// </remarks>
    public class LogFrame : SyncFrame
    {
        private readonly LogLevel level;
        private readonly string message;
        private readonly string[] parameters;

        private LoggerVariable loggerVariable;

        private LogFrame(LogLevel level, string message, string[] parameters)
        {
            this.level = level;
            this.message = message;
            this.parameters = parameters;
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="LogLevel.Trace" /> level and given
        /// message.
        /// </summary>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Trace(string message, params string[] parameters)
        {
            return new LogFrame(LogLevel.Trace, message, parameters);
        }

        /// <summary>
        /// Writes directly to the given <see cref="ISourceWriter" /> a <see cref="LogFrame" /> of level <see cref="LogLevel.Trace" />, enabling the
        /// re-use of the logic within a custom <see cref="Frame" />.
        /// </summary>
        /// <param name="method">The method this line is being written to.</param>
        /// <param name="writer">The writer to output the given log message to.</param>
        /// <param name="loggerVariable">The logger variable that represents the method's injected <see cref="ILogger" />.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        public static void Trace(GeneratedMethod method, ISourceWriter writer, Variable loggerVariable, string message, params string[] parameters)
        {
            WriteLog(method, writer, loggerVariable, LogLevel.Trace, message, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="LogLevel.Debug" /> level and given
        /// message.
        /// </summary>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Debug(string message, params string[] parameters)
        {
            return new LogFrame(LogLevel.Debug, message, parameters);
        }

        /// <summary>
        /// Writes directly to the given <see cref="ISourceWriter" /> a <see cref="LogFrame" /> of level <see cref="LogLevel.Debug" />, enabling the
        /// re-use of the logic within a custom <see cref="Frame" />.
        /// </summary>
        /// <param name="method">The method this line is being written to.</param>
        /// <param name="writer">The writer to output the given log message to.</param>
        /// <param name="loggerVariable">The logger variable that represents the method's injected <see cref="ILogger" />.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        public static void Debug(GeneratedMethod method, ISourceWriter writer, Variable loggerVariable, string message, params string[] parameters)
        {
            WriteLog(method, writer, loggerVariable, LogLevel.Debug, message, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="LogLevel.Information" /> level and given
        /// message.
        /// </summary>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Information(string message, params string[] parameters)
        {
            return new LogFrame(LogLevel.Information, message, parameters);
        }

        /// <summary>
        /// Writes directly to the given <see cref="ISourceWriter" /> a <see cref="LogFrame" /> of level <see cref="LogLevel.Information" />, enabling the
        /// re-use of the logic within a custom <see cref="Frame" />.
        /// </summary>
        /// <param name="method">The method this line is being written to.</param>
        /// <param name="writer">The writer to output the given log message to.</param>
        /// <param name="loggerVariable">The logger variable that represents the method's injected <see cref="ILogger" />.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        public static void Information(GeneratedMethod method, ISourceWriter writer, Variable loggerVariable, string message, params string[] parameters)
        {
            WriteLog(method, writer, loggerVariable, LogLevel.Information, message, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="LogLevel.Warning" /> level and given
        /// message.
        /// </summary>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Warning(string message, params string[] parameters)
        {
            return new LogFrame(LogLevel.Warning, message, parameters);
        }

        /// <summary>
        /// Writes directly to the given <see cref="ISourceWriter" /> a <see cref="LogFrame" /> of level <see cref="LogLevel.Warning" />, enabling the
        /// re-use of the logic within a custom <see cref="Frame" />.
        /// </summary>
        /// <param name="method">The method this line is being written to.</param>
        /// <param name="writer">The writer to output the given log message to.</param>
        /// <param name="loggerVariable">The logger variable that represents the method's injected <see cref="ILogger" />.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        public static void Warning(GeneratedMethod method, ISourceWriter writer, Variable loggerVariable, string message, params string[] parameters)
        {
            WriteLog(method, writer, loggerVariable, LogLevel.Warning, message, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="LogLevel.Error" /> level and given
        /// message.
        /// </summary>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Error(string message, params string[] parameters)
        {
            return new LogFrame(LogLevel.Error, message, parameters);
        }

        /// <summary>
        /// Writes directly to the given <see cref="ISourceWriter" /> a <see cref="LogFrame" /> of level <see cref="LogLevel.Error" />, enabling the
        /// re-use of the logic within a custom <see cref="Frame" />.
        /// </summary>
        /// <param name="method">The method this line is being written to.</param>
        /// <param name="writer">The writer to output the given log message to.</param>
        /// <param name="loggerVariable">The logger variable that represents the method's injected <see cref="ILogger" />.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        public static void Error(GeneratedMethod method, ISourceWriter writer, Variable loggerVariable, string message, params string[] parameters)
        {
            WriteLog(method, writer, loggerVariable, LogLevel.Error, message, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="LogLevel.Critical" /> level and given
        /// message.
        /// </summary>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Critical(string message, params string[] parameters)
        {
            return new LogFrame(LogLevel.Critical, message, parameters);
        }

        /// <summary>
        /// Writes directly to the given <see cref="ISourceWriter" /> a <see cref="LogFrame" /> of level <see cref="LogLevel.Critical" />, enabling the
        /// re-use of the logic within a custom <see cref="Frame" />.
        /// </summary>
        /// <param name="method">The method this line is being written to.</param>
        /// <param name="writer">The writer to output the given log message to.</param>
        /// <param name="loggerVariable">The logger variable that represents the method's injected <see cref="ILogger" />.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        public static void Critical(GeneratedMethod method, ISourceWriter writer, Variable loggerVariable, string message, params string[] parameters)
        {
            WriteLog(method, writer, loggerVariable, LogLevel.Critical, message, parameters);
        }

        /// <inheritdoc />
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var safeMessage = message.Replace("\"", "\\\"");

            if (loggerVariable.Logger.IsEnabled(level))
            {
                var methodCall = $"{loggerVariable}.{nameof(ILogger.Log)}";
                var logLevel = Variable.StaticFrom<LogLevel>(level.ToString());

                writer.WriteLine(parameters.Length == 0
                    ? $"{methodCall}({logLevel}, \"{safeMessage}\");"
                    : $"{methodCall}({logLevel}, \"{safeMessage}\", {string.Join(", ", parameters)});");
            }

            Next?.GenerateCode(method, writer);
        }

        /// <inheritdoc />
        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            loggerVariable = (LoggerVariable)chain.FindVariable(typeof(ILogger));

            yield return loggerVariable;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Log.{level}({message})";
        }

        private static void WriteLog(
            GeneratedMethod method,
            ISourceWriter writer,
            Variable loggerVariable,
            LogLevel level,
            string message,
            params string[] parameters)
        {
            if (!(loggerVariable is LoggerVariable))
            {
                throw new InvalidOperationException($"Cannot pass {loggerVariable} that is not of type {nameof(LoggerVariable)}.");
            }

            var frame = new LogFrame(level, message, parameters) {loggerVariable = (LoggerVariable)loggerVariable};
            frame.GenerateCode(method, writer);
        }
    }
}
