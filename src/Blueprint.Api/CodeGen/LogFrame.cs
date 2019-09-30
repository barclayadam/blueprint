using System.Collections.Generic;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using NLog;

namespace Blueprint.Api.CodeGen
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that will output a log message to the configured NLog <see cref="Logger"/>
    /// at the specified level.
    /// </summary>
    /// <remarks>
    /// The Logger used in output code is found by searching for a configured <see cref="LoggerVariable"/>
    /// and will be used to check whether the log level is enabled, <b>not</b> outputting any logging in the case of
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
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="LogLevel.Info" /> level and given
        /// message.
        /// </summary>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Info(string message, params string[] parameters)
        {
            return new LogFrame(LogLevel.Info, message, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="LogLevel.Warn" /> level and given
        /// message.
        /// </summary>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Warn(string message, params string[] parameters)
        {
            return new LogFrame(LogLevel.Warn, message, parameters);
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
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="LogLevel.Fatal" /> level and given
        /// message.
        /// </summary>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Fatal(string message, params string[] parameters)
        {
            return new LogFrame(LogLevel.Fatal, message, parameters);
        }

        /// <inheritdoc />
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var safeMessage = message.Replace("\"", "\\\"");

            if (loggerVariable.Logger.IsEnabled(level))
            {
                var methodCall = $"{loggerVariable}.{nameof(Logger.Log)}";
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
            loggerVariable = (LoggerVariable)chain.FindVariable(typeof(Logger));

            yield return loggerVariable;
        }
    }
}
