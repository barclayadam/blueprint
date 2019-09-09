using System;
using System.Runtime.Serialization;

namespace Blueprint.Core.Notifications
{
    /// <summary>
    /// An exception that is raised when a notification cannot be found.
    /// </summary>
    [Serializable]
    public class NotificationNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the NotificationNotFoundException class.
        /// </summary>
        public NotificationNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotificationNotFoundException class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public NotificationNotFoundException(string message)
                : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotificationNotFoundException class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="inner">The inner exception which ultimately caused this exception.</param>
        public NotificationNotFoundException(string message, Exception inner)
                : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotificationNotFoundException class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected NotificationNotFoundException(
                SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }
    }
}