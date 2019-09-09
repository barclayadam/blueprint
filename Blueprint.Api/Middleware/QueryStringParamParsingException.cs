using System;
using System.Net;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// An exception to be thrown when there is a problem with a query string parameter, that it is invalid
    /// and cannot be parsed (e.g. trying to pass "not_an_int" for an int32 property)
    /// </summary>
    [Serializable]
    public class QueryStringParamParsingException : ApiException
    {
        public QueryStringParamParsingException(string message)
                : base(message, "invalid_parameter", HttpStatusCode.BadRequest)
        {
        }
    }
}