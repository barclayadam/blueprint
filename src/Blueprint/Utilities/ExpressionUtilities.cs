using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Blueprint.Utilities;

/// <summary>
/// A number of utility methods when dealing with LINQ <see cref="Expression"/>s.
/// </summary>
public static class ExpressionUtilities
{
    /// <summary>
    /// Given a MethodCallExpression will convert all arguments into their actual runtime values.
    /// </summary>
    /// <param name="methodCallExpression">
    /// The method call expression from which to convert the arguments.
    /// </param>
    /// <returns>
    /// The actual values supplied in the given method call expression.
    /// </returns>
    public static IEnumerable<object> GetMethodCallExpressionArgumentValues(
        MethodCallExpression methodCallExpression)
    {
        Guard.NotNull(nameof(methodCallExpression), methodCallExpression);

        return methodCallExpression.Arguments.Select(GetValue);
    }

    /// <summary>
    /// Given an Expression will get the value that it represents, particularly useful for taking
    /// a MethodCallExpression and determine the actual values being passed.
    /// </summary>
    /// <param name="expression">
    /// The expression to convert to an actual value.
    /// </param>
    /// <returns>
    /// The value that the expression represents.
    /// </returns>
    public static object GetValue(Expression expression)
    {
        Guard.NotNull(nameof(expression), expression);

        var constantExpression = expression as ConstantExpression;

        if (constantExpression != null)
        {
            return constantExpression.Value;
        }

        var memberExpression = expression as MemberExpression;

        if (memberExpression != null)
        {
            return GetValueFromMemberExpression(memberExpression);
        }

        var unaryExpression = expression as UnaryExpression;

        if (unaryExpression != null)
        {
            return GetValueFromMemberExpression(unaryExpression.Operand as MemberExpression);
        }

        throw new InvalidOperationException(
            $"Trying to get a constant value from an expression of type {expression.GetType().Name}. Make sure it is either a ConstantExpression or a MemberExpression");
    }

    private static object GetValueFromMemberExpression(MemberExpression member)
    {
        Guard.NotNull(nameof(member), member);

        var objectMember = Expression.Convert(member, typeof(object));

        var getter = Expression.Lambda<Func<object>>(objectMember).Compile();

        return getter();
    }
}