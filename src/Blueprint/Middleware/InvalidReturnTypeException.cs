using System;

namespace Blueprint.Middleware;

/// <summary>
/// Exception thrown when an inline method handler has an incompatible return type declared.
/// </summary>
public class InvalidReturnTypeException : Exception
{
    /// <summary>
    /// Instantiates a new instance of the <see cref="InvalidReturnTypeException" /> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public InvalidReturnTypeException(string message)
        : base(message)
    {
    }
}