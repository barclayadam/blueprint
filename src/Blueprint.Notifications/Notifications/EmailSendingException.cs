using System;
using System.Runtime.Serialization;

namespace Blueprint.Notifications.Notifications
{
    /// <summary>
    /// An exception raised when the sending of email fails.
    /// </summary>
    [Serializable]
    public class EmailSendingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the EmailSendingException class. 
        /// </summary>
        public EmailSendingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the EmailSendingException class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public EmailSendingException(string message)
                : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the EmailSendingException class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception.</param>
        public EmailSendingException(string message, Exception inner)
                : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the EmailSendingException class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected EmailSendingException(SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }
    }
}