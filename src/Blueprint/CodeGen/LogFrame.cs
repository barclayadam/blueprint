using System;
using System.Linq;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Microsoft.Extensions.Logging;

namespace Blueprint.CodeGen
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
        private readonly object[] parameters;

        private LogFrame(LogLevel level, string message, object[] parameters)
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
        public static LogFrame Trace(string message, params object[] parameters)
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
        public static LogFrame Debug(string message, params object[] parameters)
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
        public static LogFrame Information(string message, params object[] parameters)
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
        public static LogFrame Warning(string message, params object[] parameters)
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
        public static LogFrame Error(string message, params object[] parameters)
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
        public static LogFrame Critical(string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Critical, message, parameters);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Log.{level}(\"{message}\")";
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            var loggerVariable = variables.FindVariable(typeof(ILogger));
            var safeMessage = SafeValue(message);

            var methodCall = $"{loggerVariable}.{nameof(ILogger.Log)}";
            var logLevel = Variable.StaticFrom<LogLevel>(level.ToString());

            var safeParameters = parameters.Select(SafeValue);

            writer.WriteLine(parameters.Length == 0
                ? $"{methodCall}({logLevel}, {safeMessage});"
                : $"{methodCall}({logLevel}, {safeMessage}, {string.Join(", ", safeParameters)});");

            next();
        }

        private static string SafeValue(object value)
        {
            while (true)
            {
                if (value is string s)
                {
                    return $"\"{s.Replace("\"", "\\\"")}\"";
                }

                if (value is Variable v)
                {
                    return v.Usage;
                }

                value = value.ToString();
            }
        }
    }
}
