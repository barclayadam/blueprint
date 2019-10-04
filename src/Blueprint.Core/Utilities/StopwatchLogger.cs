using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Blueprint.Core.Utilities
{
    /// <summary>
    /// Provides a simple means of tracking the time it takes to perform
    /// actions through logging of a start and end message, which will
    /// contain a duration in milliseconds of how long something took.
    /// </summary>
    public static class StopwatchLogger
    {
        /// <summary>
        /// Starts a new timer, logging the message immediately and then, once disposed, the
        /// message again with `time_taken_ms={[0-9]?}` appended.
        /// </summary>
        /// <param name="log">The log to which to output this information.</param>
        /// <param name="message">The message to be logged before and after the action.</param>
        /// <param name="args">The arguments to be used in formatting of the message.</param>
        /// <returns>A disposable object that, once disposed, will log the time between creating and
        /// disposing.</returns>
        public static IDisposable LogTimeWrapper(this ILogger log, string message, params object[] args)
        {
            return new StopwatchLoggerDisposable(log, message, true, args);
        }

        /// <summary>
        /// Starts a new timer, only logging message once disposed, with the
        /// with `time_taken_ms={[0-9]?}` appended.
        /// </summary>
        /// <param name="log">The log to which to output this information.</param>
        /// <param name="message">The message to be logged before and after the action.</param>
        /// <param name="args">The arguments to be used in formatting of the message.</param>
        /// <returns>A disposable object that, once disposed, will log the time between creating and
        /// disposing.</returns>
        public static IDisposable LogTime(this ILogger log, string message, params object[] args)
        {
            return new StopwatchLoggerDisposable(log, message, false, args);
        }

        private class StopwatchLoggerDisposable : IDisposable
        {
            private readonly ILogger log;
            private readonly string message;
            private readonly bool logStart;

            private readonly Stopwatch stopwatch;

            internal StopwatchLoggerDisposable(ILogger log, string messageFormat, bool logStart, params object[] args)
            {
                this.log = log;
                this.logStart = logStart;

                message = messageFormat.Fmt(args);

                if (logStart)
                {
                    log.LogInformation("[START] " + message);
                }

                stopwatch = new Stopwatch();
                stopwatch.Start();
            }

            public void Dispose()
            {
                stopwatch.Stop();

                log.LogInformation(
                    "{0}{1} time_taken_ms={2}",
                    logStart ? "[STOP] " : string.Empty,
                    message,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
