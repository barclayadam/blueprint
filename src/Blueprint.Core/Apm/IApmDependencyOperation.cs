using System;

namespace Blueprint.Core.Apm
{
    /// <summary>
    /// Represents a dependency that is being tracked for APM purposes. An instance can be created
    /// by using <see cref="IApmTool.TrackDependencyAsync" />, and will, when disposed, log the dependency
    /// using the configured APM implementation.
    /// </summary>
    public interface IApmDependencyOperation
    {
        /// <summary>
        /// Marks this dependency as successful, optionally setting the result code (which is typically a HTTP
        /// code, even for non-http) of the result.
        /// </summary>
        /// <param name="resultCode">The (optional) result code to set.</param>
        void MarkSuccess(string resultCode = "200");

        /// <summary>
        /// Marks this dependency as a failure with the given (usually HTTP-like) result code.
        /// </summary>
        /// <param name="resultCode">The result code of this dependency failure.</param>
        /// <param name="exception">The (optional) exception that represents the dependency failure.</param>
        void MarkFailure(string resultCode, Exception exception = null);
    }
}