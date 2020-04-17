using System;
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
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Trace" /> level and given
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
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Debug" /> level and given
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
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Information" /> level and given
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
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Warning" /> level and given
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
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Error" /> level and given
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
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Critical" /> level and given
        /// message.
        /// </summary>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Critical(string message, params string[] parameters)
        {
            return new LogFrame(LogLevel.Critical, message, parameters);
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            loggerVariable = (LoggerVariable)variables.FindVariable(typeof(ILogger));
            var safeMessage = message.Replace("\"", "\\\"");

            if (loggerVariable.Logger.IsEnabled(level))
            {
                var methodCall = $"{loggerVariable}.{nameof(ILogger.Log)}";
                var logLevel = Variable.StaticFrom<LogLevel>(level.ToString());

                writer.WriteLine(parameters.Length == 0
                    ? $"{methodCall}({logLevel}, \"{safeMessage}\");"
                    : $"{methodCall}({logLevel}, \"{safeMessage}\", {string.Join(", ", parameters)});");
            }

            next();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Log.{level}(\"{message}\")";
        }
    }
}
