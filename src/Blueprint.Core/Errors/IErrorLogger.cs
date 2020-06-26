using System;
using System.Threading.Tasks;
using Blueprint.Core.Authorisation;

namespace Blueprint.Core.Errors
{
    /// <summary>
    /// An error logger takes unhandled exceptions and attempts to record / log them,
    /// using a set of <see cref="IExceptionFilter"/>s and <see cref="IExceptionSink"/>s.
    /// </summary>
    public interface IErrorLogger
    {
        /// <summary>
        /// Attempts to record the given exception, determining first whether it should be
        /// recorded at all (not recorded in essence suppresses the exception).
        /// </summary>
        /// <param name="exception">The exception to attempt to log.</param>
        /// <param name="errorData">Extra data to attach to the Exception object via. it's Data property, used in exception sinks to
        /// add metadata useful for diagnostics purposes (likely an anonymous object that will be converted to a string).</param>
        /// <param name="identifier">A user identifier that will be sent along with this error to track by-user.</param>
        /// <returns>The status of recording, useful to determine the next course of action depending
        /// on application.</returns>
        ValueTask<ErrorLogStatus> LogAsync(
            Exception exception,
            object errorData = default,
            UserExceptionIdentifier identifier = default);

        /// <summary>
        /// Attempts to record the given exception, determining first whether it should be
        /// recorded at all (not recorded in essence suppresses the exception).
        /// </summary>
        /// <param name="exceptionMessage">The exception message to attempt to log.</param>
        /// <param name="errorData">Custom data that will be associated with the log entry for this exception (likely an anonymous object).</param>
        /// <param name="identifier">A user identifier that will be sent along with this error to track by-user.</param>
        /// <returns>The status of recording, useful to determine the next course of action depending
        /// on application.</returns>
        ValueTask<ErrorLogStatus> LogAsync(
            string exceptionMessage,
            object errorData = default,
            UserExceptionIdentifier identifier = default);

        /// <summary>
        /// Gets a value indicating whether this exception should be ignored and not logged. This is useful
        /// for handling "known" exception that are expected in the course of running an application, but would be
        /// noise to log all the time.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns>Whether the given exception should be ignored for logging purposes.</returns>
        bool ShouldIgnore(Exception exception);
    }
}
