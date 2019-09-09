using System;
using NHibernate;
using NLog;

namespace Blueprint.Core.Data.NHibernate
{
    /// <summary>
    /// An implementation of an <see cref="IInternalLogger" /> that allows NHibernate logging
    /// to be directly to NLog.
    /// </summary>
    public class NLogLogger : IInternalLogger
    {
        private readonly Logger log;
        
        public NLogLogger(Logger log)
        {
            this.log = log;
        }
        
        /// <summary>
        /// Gets a value indicating whether this debugging is enabled for <c>debug</c>
        /// messages.
        /// </summary>
        public bool IsDebugEnabled => log.IsDebugEnabled;

        /// <summary>
        /// Gets a value indicating whether this debugging is enabled for <c>error</c>
        /// messages.
        /// </summary>
        public bool IsErrorEnabled => log.IsErrorEnabled;

        /// <summary>
        /// Gets a value indicating whether this debugging is enabled for <c>fatal</c>
        /// messages.
        /// </summary>
        public bool IsFatalEnabled => log.IsFatalEnabled;

        /// <summary>
        /// Gets a value indicating whether this debugging is enabled for <c>info</c>
        /// messages.
        /// </summary>
        public bool IsInfoEnabled => log.IsInfoEnabled;

        /// <summary>
        /// Gets a value indicating whether this debugging is enabled for <c>warn</c>
        /// messages.
        /// </summary>
        public bool IsWarnEnabled => log.IsWarnEnabled;

        /// <summary>
        /// Logs a message and exception at a <c>debug</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception to be logged.</param>
        public void Debug(object message, Exception exception)
        {
            if (message == null || exception == null)
                return;

            log.Debug(exception, message.ToString());
        }

        /// <summary>
        /// Logs a message at a <c>debug</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public void Debug(object message)
        {
            if (message == null)
                return;

            log.Debug(message.ToString());
        }

        /// <summary>
        /// Logs a message at a <c>debug</c> level, using the optional arguments
        /// to fortmat the string (<see cref="string.Format(string,object)" />.
        /// </summary>
        /// <param name="format">The message format to be used.</param>
        /// <param name="args">The arguments to inject into the format strng.</param>
        public void DebugFormat(string format, params object[] args)
        {
            log.Debug(format, args);
        }

        /// <summary>
        /// Logs a message and exception at a <c>error</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception to be logged.</param>
        public void Error(object message, Exception exception)
        {
            if (message == null || exception == null)
                return;

            log.Error(exception, message.ToString());
        }

        /// <summary>
        /// Logs a message at a <c>error</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public void Error(object message)
        {
            if (message == null)
                return;

            log.Error(message.ToString());
        }

        /// <summary>
        /// Logs a message at a <c>error</c> level, using the optional arguments
        /// to fortmat the string (<see cref="string.Format(string,object)" />.
        /// </summary>
        /// <param name="format">The message format to be used.</param>
        /// <param name="args">The arguments to inject into the format strng.</param>
        public void ErrorFormat(string format, params object[] args)
        {
            log.Error(format, args);
        }

        /// <summary>
        /// Logs a message and exception at a <c>fatal</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception to be logged.</param>
        public void Fatal(object message, Exception exception)
        {
            if (message == null || exception == null)
                return;

            log.Fatal(exception, message.ToString());
        }

        /// <summary>
        /// Logs a message at a <c>fatal</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public void Fatal(object message)
        {
            if (message == null)
                return;

            log.Fatal(message.ToString());
        }

        /// <summary>
        /// Logs a message and exception at a <c>info</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception to be logged.</param>
        public void Info(object message, Exception exception)
        {
            if (message == null || exception == null)
                return;

            log.Info(exception, message.ToString());
        }

        /// <summary>
        /// Logs a message at a <c>info</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public void Info(object message)
        {
            if (message == null)
                return;

            log.Info(message.ToString());
        }

        /// <summary>
        /// Logs a message at a <c>info</c> level, using the optional arguments
        /// to fortmat the string (<see cref="string.Format(string,object)" />.
        /// </summary>
        /// <param name="format">The message format to be used.</param>
        /// <param name="args">The arguments to inject into the format strng.</param>
        public void InfoFormat(string format, params object[] args)
        {
            log.Info(format, args);
        }

        /// <summary>
        /// Logs a message and exception at a <c>warn</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception to be logged.</param>
        public void Warn(object message, Exception exception)
        {
            if (message == null || exception == null)
                return;

            log.Warn(exception, message.ToString());
        }

        /// <summary>
        /// Logs a message at a <c>warn</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public void Warn(object message)
        {
            if (message == null)
                return;

            log.Warn(message.ToString());
        }

        /// <summary>
        /// Logs a message at a <c>warn</c> level, using the optional arguments
        /// to fortmat the string (<see cref="string.Format(string,object)" />.
        /// </summary>
        /// <param name="format">The message format to be used.</param>
        /// <param name="args">The arguments to inject into the format strng.</param>
        public void WarnFormat(string format, params object[] args)
        {
            log.Warn(format, args);
        }
    }
}