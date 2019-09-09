using System;
using Microsoft.AspNetCore.Http;

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
        /// <param name="errorData">Custom data that will be associated with the log entry for this exception (likely an anonymous object).</param>
        /// <param name="httpContext">The Asp.Net Core HttpContext, if available.</param>
        /// <param name="identifier">A user identifier that will be sent along with this error to track by-user.</param>
        /// <returns>The status of recording, useful to determine the next course of action depending
        /// on application.</returns>
        ErrorLogStatus Log(
            Exception exception,
            object errorData = default,
            HttpContext httpContext = default,
            UserExceptionIdentifier identifier = default);

        /// <summary>
        /// Attempts to record the given exception, determining first whether it should be
        /// recorded at all (not recorded in essence suppresses the exception).
        /// </summary>
        /// <param name="exceptionMessage">The exception message to attempt to log.</param>
        /// <param name="errorData">Custom data that will be associated with the log entry for this exception (likely an anonymous object).</param>
        /// <param name="httpContext">The Asp.Net Core HttpContext, if available.</param>
        /// <param name="identifier">A user identifier that will be sent along with this error to track by-user.</param>
        /// <returns>The status of recording, useful to determine the next course of action depending
        /// on application.</returns>
        ErrorLogStatus Log(
            string exceptionMessage,
            object errorData = default,
            HttpContext httpContext = default,
            UserExceptionIdentifier identifier = default);

        bool ShouldIgnore(Exception exception);
    }
}
