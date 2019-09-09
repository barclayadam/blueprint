using System;

namespace Blueprint.Core.Errors
{
    /// <summary>
    /// Provides a mechanism for ignoring exceptions for logging purposes, to indicate
    /// that an exception is 'normal' behaviour that does not need to have an alert raised.
    /// </summary>
    public interface IExceptionFilter
    {
        /// <summary>
        /// Determines whether the given exception should be logged, providing both
        /// the type and actual exception to avoid each ruleset having to perform the
        /// <see cref="object.GetType" /> method call.
        /// </summary>
        /// <param name="exceptionType">The type of the exception to check.</param>
        /// <param name="exception">The actual exception.</param>
        /// <returns>Whether this exception should be ignored for alerting purposes.</returns>
        bool ShouldIgnore(Type exceptionType, Exception exception);
    }
}