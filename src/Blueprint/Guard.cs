using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Blueprint;

/// <summary>
/// Common guard class for argument validation.
/// </summary>
[DebuggerStepThrough]
public static class Guard
{
    /// <summary>
    /// Ensures the given <paramref name="value"/> is not null.
    /// Throws <see cref="ArgumentNullException"/> otherwise.
    /// </summary>
    /// <param name="argumentName">The name of the argument being checked, will be used in exception.</param>
    /// <param name="value">The value to check.</param>
    /// <exception cref="System.ArgumentException">The <paramref name="value"/> is null.</exception>
    public static void NotNull([InvokerParameterName]string argumentName, [NoEnumeration] object value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(argumentName, "Parameter cannot be null.");
        }
    }

    /// <summary>
    /// Ensures the given string <paramref name="value"/> is not null or empty.
    /// Throws <see cref="ArgumentNullException"/> in the first case, or
    /// <see cref="ArgumentException"/> in the latter.
    /// </summary>
    /// <param name="argumentName">The name of the argument being checked, will be used in exception.</param>
    /// <param name="value">The value to check.</param>
    /// <exception cref="System.ArgumentException">The <paramref name="value"/> is null or an empty string.</exception>
    public static void NotNullOrEmpty([InvokerParameterName]string argumentName, string value)
    {
        NotNull(argumentName, value);

        if (value.Length == 0)
        {
            throw new ArgumentException("Parameter cannot be empty.", argumentName);
        }
    }

    public static void EnumDefined<T>([InvokerParameterName]string argumentName, T value)
    {
        if (!Enum.IsDefined(typeof(T), value))
        {
            throw new ArgumentException(
                $"Parameter must be enum of type {typeof(T).Name}. Was {value}.",
                argumentName);
        }
    }

    public static void GreaterThanOrEqual<T>([InvokerParameterName]string argumentName, T value, T referencePoint) where T : IComparable
    {
        if (value.CompareTo(referencePoint) < 0)
        {
            throw new ArgumentException($"Parameter must be greater than or equal {referencePoint}. Was {value}.", argumentName);
        }
    }

    public static void GreaterThan<T>([InvokerParameterName]string argumentName, T value, T referencePoint) where T : IComparable
    {
        if (value.CompareTo(referencePoint) <= 0)
        {
            throw new ArgumentException($"Parameter must be greater than {referencePoint}. Was {value}.", argumentName);
        }
    }

    public static void LessThanOrEqual<T>([InvokerParameterName]string argumentName, T value, T referencePoint) where T : IComparable
    {
        if (value.CompareTo(referencePoint) > 0)
        {
            throw new ArgumentException($"Parameter must be less than or equal {referencePoint}. Was {value}.", argumentName);
        }
    }

    public static void LessThan<T>([InvokerParameterName]string argumentName, T value, T referencePoint) where T : IComparable
    {
        if (value.CompareTo(referencePoint) >= 0)
        {
            throw new ArgumentException($"Parameter must be less than {referencePoint}. Was {value}.", argumentName);
        }
    }

    public static void NotEmpty<T>([InvokerParameterName]string argumentName, IEnumerable<T> value)
    {
        if (!value.Any())
        {
            throw new ArgumentException("Parameter must contain at least one element.", argumentName);
        }
    }

    public static void IsEqual<T>([InvokerParameterName]string argumentName, T value, T reference, string message)
    {
        if (!Equals(value, reference))
        {
            throw new ArgumentException(message, argumentName);
        }
    }

    public static void IsTrue([InvokerParameterName]string argumentName, bool value, string message)
    {
        if (!value)
        {
            throw new ArgumentException(message, argumentName);
        }
    }

    public static void IsFalse([InvokerParameterName]string argumentName, bool value, string message)
    {
        if (value)
        {
            throw new ArgumentException(message, argumentName);
        }
    }

    public static void MustHaveLengthLessThanOrEqualTo<T>(IEnumerable<T> propertyValue, int maxLength, string propertyName)
    {
        NotNullOrEmpty(nameof(propertyName), propertyName);

        var nullablePropertyValue = propertyValue;

        if (nullablePropertyValue == null || nullablePropertyValue.Count() <= maxLength)
        {
            return;
        }

        throw new ArgumentException($"{propertyName} must have a length less than or equal to {maxLength}", propertyName);
    }
}