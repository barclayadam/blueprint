using System;
using Blueprint.Middleware;
using Blueprint.Validation;

namespace Blueprint.Testing;

/// <summary>
/// Provides extension methods for making tests more succinct when dealing with <see cref="OperationResult"/>s.
/// </summary>
public static class ResultTestingExtensions
{
    /// <summary>
    /// Checks that this <see cref="OperationResult" /> is a valid result of type <typeparamref name="T" />, throwing exceptions for
    /// known failure result types (i.e. <see cref="ValidationFailedOperationResult" /> and <see cref="UnhandledExceptionOperationResult"/>,
    /// finally returning the correct result.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <typeparam name="T">The expected content type.</typeparam>
    /// <returns>The converted result.</returns>
    /// <exception cref="ValidationException">If this result is actually <see cref="ValidationFailedOperationResult"/>.</exception>
    /// <exception cref="InvalidOperationException">If this result is actually <see cref="UnhandledExceptionOperationResult"/>.</exception>
    public static T ShouldBeOperationResultType<T>(this OperationResult result)
    {
        if (result is ValidationFailedOperationResult validationFailedResult)
        {
            throw new ValidationException("Validation failed", validationFailedResult.Errors);
        }

        if (result is UnhandledExceptionOperationResult exceptionOperationResult)
        {
            exceptionOperationResult.Rethrow();

            return default;
        }

        if (result is T expectedResult)
        {
            return expectedResult;
        }

        throw new InvalidOperationException($"Unhandled result type {result.GetType().Name}, expected {typeof(T).Name}");
    }

    /// <summary>
    /// Checks that this <see cref="OperationResult" /> is a valid result of type <see cref="OkResult" /> that has the
    /// correct <see cref="OkResult.Content" /> property of the specified type, throwing exceptions for known failure
    /// result types ands returning the Content as the requested type.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <typeparam name="T">The expected content type.</typeparam>
    /// <returns>The value of <see cref="OkResult.Content"/> as the expected type.</returns>
    /// <exception cref="ValidationException">If this result is actually <see cref="ValidationFailedOperationResult"/>.</exception>
    /// <exception cref="InvalidOperationException">If this result is actually <see cref="UnhandledExceptionOperationResult"/>.</exception>
    public static T ShouldBeContent<T>(this OperationResult result)
    {
        return (T)result.ShouldBeOperationResultType<OkResult>().Content;
    }
}