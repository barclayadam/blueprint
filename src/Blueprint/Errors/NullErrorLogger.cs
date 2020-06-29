using System;
using System.Threading.Tasks;
using Blueprint.Authorisation;

namespace Blueprint.Errors
{
    /// <summary>
    /// An <see cref="IErrorLogger" /> that does nothing, but will consume and mark as recorded any exceptions
    /// that are sent to it.
    /// </summary>
    public class NullErrorLogger : IErrorLogger
    {
        private static readonly ValueTask<ErrorLogStatus> RecordedStatus = new ValueTask<ErrorLogStatus>(ErrorLogStatus.Recorded);

        /// <inheritdoc />
        /// <remarks>
        /// This method does nothing but return <see cref="ErrorLogStatus.Recorded" />.
        /// </remarks>
        public ValueTask<ErrorLogStatus> LogAsync(Exception exception, object errorData = default, UserExceptionIdentifier identifier = default)
        {
            return RecordedStatus;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method does nothing but return <see cref="ErrorLogStatus.Recorded" />.
        /// </remarks>
        public ValueTask<ErrorLogStatus> LogAsync(string exceptionMessage, object errorData = default, UserExceptionIdentifier identifier = default)
        {
            return RecordedStatus;
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
