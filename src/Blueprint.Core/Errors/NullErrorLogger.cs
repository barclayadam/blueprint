using System;
using Blueprint.Core.Authorisation;

namespace Blueprint.Core.Errors
{
    /// <summary>
    /// An <see cref="IErrorLogger" /> that does nothing, but will consume and mark as recorded any exceptions
    /// that are sent to it.
    /// </summary>
    public class NullErrorLogger : IErrorLogger
    {
        /// <inheritdoc />
        /// <remarks>
        /// This method does nothing but return <see cref="ErrorLogStatus.Recorded" />.
        /// </remarks>
        public ErrorLogStatus Log(Exception exception, UserExceptionIdentifier identifier = default)
        {
            return ErrorLogStatus.Recorded;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method does nothing but return <see cref="ErrorLogStatus.Recorded" />.
        /// </remarks>
        public ErrorLogStatus Log(string exceptionMessage, object errorData = default, UserExceptionIdentifier identifier = default)
        {
            return ErrorLogStatus.Recorded;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method does nothing but return <c>false</c>.
        /// </remarks>
        public bool ShouldIgnore(Exception exception)
        {
            return false;
        }
    }
}
