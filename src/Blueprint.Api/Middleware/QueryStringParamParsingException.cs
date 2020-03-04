using System;
using System.Net;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// An exception to be thrown when there is a problem with a query string parameter, that it is invalid
    /// and cannot be parsed (e.g. trying to pass "not_an_int" for an int32 property).
    /// </summary>
    [Serializable]
    public class QueryStringParamParsingException : ApiException
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="QueryStringParamParsingException" /> class.
        /// </summary>
        /// <param name="exception">The exception that caused the exception.</param>
        /// <param name="message">The message to be shown to the user through problem report.</param>
        public QueryStringParamParsingException(Exception exception, string message)
            : base(message, "invalid_parameter", exception, HttpStatusCode.BadRequest)
        {
        }
    }
}
