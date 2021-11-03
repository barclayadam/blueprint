using System;
using System.Linq;
using System.Text.RegularExpressions;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;
using Blueprint.Utilities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Blueprint.CodeGen
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that will output a log message to the configured logger (<see cref="ILogger"/>)
    /// for the operation at the specified level.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The logger used in output code is found by searching for a configured <see cref="LoggerVariable"/>
    /// and will be used to check whether the log level is enabled, <b>not</b> outputting any logging code in the case of
    /// the log level being turned off.
    /// </para>
    /// <para>
    /// The generated code makes use of <see cref="LoggerMessage.Define" /> to create optimised delegates for rendering the
    /// log message and reducing allocations. A static field will be created and used.
    /// </para>
    /// </remarks>
    public class LogFrame : SyncFrame
    {
        private static readonly Regex _invalidVariableCharacters = new Regex("[^a-zA-Z]", RegexOptions.Compiled);

        private static int _eventIdCount = 1;

        [CanBeNull]
        private readonly Variable _exceptionVariable;
        private readonly LogLevel _level;
        private readonly string _message;
        private readonly object[] _parameters;

        private readonly StaticField _actionVariable;

        private LogFrame(LogLevel level, string message, [CanBeNull] Variable exceptionVariable, object[] parameters)
            : this(level, new EventId(_eventIdCount++, message.Replace("\"", "\\\"")), message, exceptionVariable, parameters)
        {
        }

        private LogFrame(LogLevel level, EventId eventId, string message, [CanBeNull] Variable exceptionVariable, object[] parameters)
        {
            this._level = level;
            this._message = message;
            this._exceptionVariable = exceptionVariable;
            this._parameters = parameters;

            var argTypes = parameters.Select(p => p switch
            {
                Variable v => v.VariableType,
                _ => p.GetType(),
            }).ToList();

            var argsWithLoggerAndException = new[]
            {
                typeof(ILogger),
            }.Concat(argTypes).Concat(new [] { typeof(Exception) }).ToArray();

            var actionType = argsWithLoggerAndException.Length switch
            {
                1 => typeof(Action<>),
                2 => typeof(Action<,>),
                3 => typeof(Action<,,>),
                4 => typeof(Action<,,,>),
                5 => typeof(Action<,,,,>),
                6 => typeof(Action<,,,,,>),
                7 => typeof(Action<,,,,,,>),
                8 => typeof(Action<,,,,,,,>),
                9 => typeof(Action<,,,,,,,,>),
                10 => typeof(Action<,,,,,,,,,>),
                _ => throw new ArgumentOutOfRangeException("Cannot log messages with more than 8 placeholders"),
            };

            var genericArguments = argTypes.Count == 0 ? string.Empty : $"<{string.Join(", ", argTypes)}>";

            // LoggerMessage.Define<[args]>([level], [eventId], [message])
            var initializer = $@"{typeof(LoggerMessage).FullNameInCode()}.{nameof(LoggerMessage.Define)}{genericArguments}(" +
                              $"{typeof(LogLevel).FullNameInCode()}.{level}, " +
                              $"new {typeof(EventId).FullNameInCode()}({eventId.Id}, \"{eventId.Name}\"), " +
                              $"{SafeValue(this._message)})";

            this._actionVariable = new StaticField(
                actionType.MakeGenericType(argsWithLoggerAndException),
                $"{_invalidVariableCharacters.Replace(eventId.Name, string.Empty).ToPascalCase()}",
                initializer);
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
            return new LogFrame(LogLevel.Trace, message, null, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Trace" /> level and given
        /// message.
        /// </summary>
        /// <param name="eventId">An event id that should be used to identify this log message.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Trace(EventId eventId, string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Trace, eventId, message, null, parameters);
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
            return new LogFrame(LogLevel.Debug, message, null, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Debug" /> level and given
        /// message.
        /// </summary>
        /// <param name="eventId">An event id that should be used to identify this log message.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Debug(EventId eventId, string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Debug, eventId, message, null, parameters);
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
            return new LogFrame(LogLevel.Information, message, null, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Information" /> level and given
        /// message.
        /// </summary>
        /// <param name="eventId">An event id that should be used to identify this log message.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Information(EventId eventId, string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Information, eventId, message, null, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Information" /> level and given
        /// message.
        /// </summary>
        /// <param name="exceptionVariable">A variable pointing to an exception to record.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Information(Variable exceptionVariable, string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Information, message, exceptionVariable, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Information" /> level and given
        /// message.
        /// </summary>
        /// <param name="exceptionVariable">A variable pointing to an exception to record.</param>
        /// <param name="eventId">An event id that should be used to identify this log message.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Information(Variable exceptionVariable, EventId eventId, string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Information, eventId, message, exceptionVariable, parameters);
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
            return new LogFrame(LogLevel.Warning, message, null, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Warning" /> level and given
        /// message.
        /// </summary>
        /// <param name="eventId">An event id that should be used to identify this log message.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Warning(EventId eventId, string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Warning, eventId, message, null, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Warning" /> level and given
        /// message.
        /// </summary>
        /// <param name="exceptionVariable">A variable pointing to an exception to record.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Warning(Variable exceptionVariable, string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Warning, message, exceptionVariable, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Warning" /> level and given
        /// message.
        /// </summary>
        /// <param name="exceptionVariable">A variable pointing to an exception to record.</param>
        /// <param name="eventId">An event id that should be used to identify this log message.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Warning(Variable exceptionVariable, EventId eventId, string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Warning, eventId, message, exceptionVariable, parameters);
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
            return new LogFrame(LogLevel.Error, message, null, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Error" /> level and given
        /// message.
        /// </summary>
        /// <param name="eventId">An event id that should be used to identify this log message.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Error(EventId eventId, string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Error, eventId, message, null, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Error" /> level and given
        /// message.
        /// </summary>
        /// <param name="exceptionVariable">A variable pointing to an exception to record.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Error(Variable exceptionVariable, string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Error, message, exceptionVariable, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Error" /> level and given
        /// message.
        /// </summary>
        /// <param name="exceptionVariable">A variable pointing to an exception to record.</param>
        /// <param name="eventId">An event id that should be used to identify this log message.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Error(Variable exceptionVariable, EventId eventId, string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Error, eventId, message, exceptionVariable, parameters);
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
            return new LogFrame(LogLevel.Critical, message, null, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Critical" /> level and given
        /// message.
        /// </summary>
        /// <param name="eventId">An event id that should be used to identify this log message.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Critical(EventId eventId, string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Critical, eventId, message, null, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Critical" /> level and given
        /// message.
        /// </summary>
        /// <param name="exceptionVariable">A variable pointing to an exception to record.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Critical(Variable exceptionVariable, string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Critical, message, exceptionVariable, parameters);
        }

        /// <summary>
        /// Constructs a new <see cref="LogFrame" /> with the <see cref="Microsoft.Extensions.Logging.LogLevel.Critical" /> level and given
        /// message.
        /// </summary>
        /// <param name="exceptionVariable">A variable pointing to an exception to record.</param>
        /// <param name="eventId">An event id that should be used to identify this log message.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="parameters">The (optional) parameter to place in to the message (as code snippets, NOT necessarily values).</param>
        /// <returns>A new <see cref="LogFrame"/>.</returns>
        public static LogFrame Critical(Variable exceptionVariable, EventId eventId, string message, params object[] parameters)
        {
            return new LogFrame(LogLevel.Critical, eventId, message, exceptionVariable, parameters);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Log.{this._level}(\"{this._message}\")";
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            method.GeneratedType.AllStaticFields.Add(this._actionVariable);

            var loggerVariable = variables.FindVariable(typeof(ILogger));

            var safeParameters = this._parameters.Select(SafeValue);

            var exceptionValue = this._exceptionVariable == null ? "null" : this._exceptionVariable.Usage;

            writer.WriteLine(this._parameters.Length == 0
                ? $"{this._actionVariable}({loggerVariable}, {exceptionValue});"
                : $"{this._actionVariable}({loggerVariable}, {string.Join(", ", safeParameters)}, {exceptionValue});");

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
