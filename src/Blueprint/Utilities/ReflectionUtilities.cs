﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Blueprint.Utilities;

/// <summary>
/// A number of utility methods when dealing with the reflection API.
/// </summary>
public static class ReflectionUtilities
{
    /// <summary>
    /// If the given type is <see cref="Nullable" /> that gets the "inner" type, otherwise
    /// returns the input.
    /// </summary>
    /// <param name="type">The type to potentially unwrap.</param>
    /// <returns>The unwrapped type.</returns>
    public static Type GetUnderlyingTypeIfNullable(Type type)
    {
        if (type == null)
        {
            return null;
        }

        return Nullable.GetUnderlyingType(type) ?? type;
    }

    /// <summary>
    /// Converts the given <see cref="Type" /> in to a "pretty" name, one that has the actual generic
    /// arguments output instead of the usual arity-named base (i.e. <c>IGeneric`1</c>).
    /// </summary>
    /// <param name="t">The type to print.</param>
    /// <returns>A nice name of the given type.</returns>
    public static string PrettyTypeName(Type t)
    {
        if (t.IsArray)
        {
            return PrettyTypeName(t.GetElementType()) + "[]";
        }

        if (t.IsGenericType)
        {
            var baseGenericName = t.Name.Substring(0, t.Name.LastIndexOf("`", StringComparison.InvariantCulture));
            var arguments = string.Join(", ", t.GetGenericArguments().Select(PrettyTypeName));

            return $"{baseGenericName}<{arguments}>";
        }

        return t.Name;
    }

    /// <summary>
    /// Provides a means to call a generic method where the type parameters are not known at compile time, such that
    /// if the method was called normally the generic type parameter inferred would be too 'loose'.
    /// </summary>
    /// <remarks>
    /// This method will also ensure that should an exception be thrown it will maintain its original stack trace, the
    /// use of this method becomes transparent to the client with the exception of having some entries in the stack trace
    /// representing the methods in this class. This avoids the pitfall of using reflection that means exceptions are wrapped
    /// in a TargetInvocationException and thus losing some details of the original exception.
    /// </remarks>
    /// <typeparam name="T">The type of the object on which to call the method, inferred.</typeparam>
    /// <param name="instance">The instance on which to call the method.</param>
    /// <param name="methodName">The name of the method to execute (use <c>nameof</c>).</param>
    /// <param name="genericTypeParameters">The type parameters that should be specified when constructing the method.</param>
    /// <param name="parameters">The parameters to pass to the method.</param>
    /// <returns>The result of the given method call.</returns>
    public static object CallGenericMethod<T>(this T instance, string methodName, Type[] genericTypeParameters, object[] parameters)
    {
        var met = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        return met.MakeGenericMethod(genericTypeParameters).Invoke(instance, parameters);
    }

    /// <summary>
    /// Provides a means to call a generic method where the type parameters are not known at compile time, such that
    /// if the method was called normally the generic type parameter inferred would be too 'loose'.
    /// </summary>
    /// <remarks>
    /// This method will also ensure that should an exception be thrown it will maintain its original stack trace, the
    /// use of this method becomes transparent to the client with the exception of having some entries in the stack trace
    /// representing the methods in this class. This avoids the pitfall of using reflection that means exceptions are wrapped
    /// in a TargetInvocationException and thus losing some details of the original exception.
    /// </remarks>
    /// <typeparam name="T">The type of the object on which to call the method, inferred.</typeparam>
    /// <typeparam name="TResult">The resulting type of the method being called, inferred.</typeparam>
    /// <param name="instance">The instance on which to call the method.</param>
    /// <param name="expression">An expression that represents the call that is to be made, including the real parameters.</param>
    /// <param name="genericTypeParameters">The type parameters that should be specified when constructing the method.</param>
    /// <returns>The result of the given method call.</returns>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1011:ConsiderPassingBaseTypesAsParameters",
        Justification = "Expression<Func<T, TResult>> used for compiler support of expression creation.")]
    public static TResult CallGenericMethodWithExplicitTypes<T, TResult>(
        this T instance, Expression<Func<T, TResult>> expression, params Type[] genericTypeParameters)
        where T : class
    {
        Guard.NotNull(nameof(instance), instance);
        Guard.NotNull(nameof(expression), expression);

        var methodCallExpression = expression.Body as MethodCallExpression;
        var concreteMethod = MakeGenericMethod(methodCallExpression, genericTypeParameters);
        var arguments = methodCallExpression.Arguments.Select(ExpressionUtilities.GetValue);

        try
        {
            return (TResult)concreteMethod.Invoke(instance, arguments.ToArray());
        }
        catch (TargetInvocationException tie)
        {
            ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
            return default;
        }
    }

    /// <summary>
    /// Provides a means to call a generic method where the type parameters are not known at compile time, such that
    /// if the method was called normally the generic type parameter inferred would be too 'loose'.
    /// </summary>
    /// <remarks>
    /// This method will also ensure that should an exception be thrown it will maintain its original stack trace, the
    /// use of this method becomes transparent to the client with the exception of having some entries in the stack trace
    /// representing the methods in this class. This avoids the pitfall of using reflection that means exceptions are wrapped
    /// in a TargetInvocationException and thus losing some details of the original exception.
    /// </remarks>
    /// <typeparam name="T">The type of the object on which to call the method, inferred.</typeparam>
    /// <param name="instance">The instance on which to call the method.</param>
    /// <param name="expression">An expression that represents the call that is to be made, including the real parameters.</param>
    /// <param name="genericTypeParameters">The type parameters that should be specified when constructing the method.</param>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1011:ConsiderPassingBaseTypesAsParameters",
        Justification = "Expression<Action<T>> used for compiler support of expression creation.")]
    public static void CallGenericMethodWithExplicitTypes<T>(
        this T instance, Expression<Action<T>> expression, params Type[] genericTypeParameters) where T : class
    {
        Guard.NotNull(nameof(instance), instance);
        Guard.NotNull(nameof(expression), expression);

        var methodCallExpression = expression.Body as MethodCallExpression;
        var concreteMethod = MakeGenericMethod(methodCallExpression, genericTypeParameters);
        var arguments = methodCallExpression.Arguments.Select(ExpressionUtilities.GetValue);

        try
        {
            concreteMethod.Invoke(instance, arguments.ToArray());
        }
        catch (TargetInvocationException tie)
        {
            ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
        }
    }

    /// <summary>
    /// Given a MethodCallExpression will make a method that represents a call to the method being referred to but
    /// with the specified generic type parameters.
    /// </summary>
    /// <param name="methodCallExpression">The method call expression that represents a generic method call.</param>
    /// <param name="genericTypeParameters">The type parameters that should be used.</param>
    /// <returns>A concrete version of the generic method given using the specified type parameters.</returns>
    private static MethodInfo MakeGenericMethod(MethodCallExpression methodCallExpression, Type[] genericTypeParameters)
    {
        Guard.NotNull(nameof(genericTypeParameters), genericTypeParameters);

        if (methodCallExpression == null)
        {
            throw new InvalidOperationException("The expression passed in does not represent a method call.");
        }

        if (!methodCallExpression.Method.IsGenericMethod)
        {
            throw new InvalidOperationException(
                $"The method the expression represents, '{methodCallExpression.Method.Name}', is not a generic method.");
        }

        var genericMethod = methodCallExpression.Method.GetGenericMethodDefinition();

        if (genericMethod.GetGenericArguments().Length != genericTypeParameters.Length)
        {
            throw new InvalidOperationException(
                $"The method '{methodCallExpression.Method.Name}' requires {genericMethod.GetGenericArguments().Length} generic type parameter(s), but {genericTypeParameters.Length} were passed in.");
        }

        return genericMethod.MakeGenericMethod(genericTypeParameters);
    }
}