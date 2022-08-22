using System;

namespace Blueprint.Middleware;

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
    /// <param name="inner">The exception that caused the exception.</param>
    /// <param name="message">The message to be shown to the user through problem report.</param>
    public QueryStringParamParsingException(Exception inner, string message)
        : base("A query string parameter could not be parsed", "invalid_parameter", message, 400, inner)
    {
    }
}