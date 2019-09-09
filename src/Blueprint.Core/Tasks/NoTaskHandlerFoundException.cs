using System;
using System.Runtime.Serialization;

namespace Blueprint.Core.Tasks
{
    /// <summary>
    /// An exception that is thrown if no task handler is found.
    /// </summary>
    [Serializable]
    public class NoTaskHandlerFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoTaskHandlerFoundException"/> class.
        /// </summary>
        public NoTaskHandlerFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoTaskHandlerFoundException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public NoTaskHandlerFoundException(string message)
                : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoTaskHandlerFoundException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="inner">
        /// The inner exception.
        /// </param>
        public NoTaskHandlerFoundException(string message, Exception inner)
                : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoTaskHandlerFoundException"/> class.
        /// </summary>
        /// <param name="info">
        /// The serialization info.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        protected NoTaskHandlerFoundException(SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }
    }
}