using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Blueprint.Utilities;

public static class TypeExtensions
{
    /// <summary>
    /// Gets a 'simple' qualified assembly name, one that contains the full type name including
    /// namespace, and the assembly name, without culture, version and token.
    /// </summary>
    /// <remarks>
    /// This method is useful for teh case where the version information could change between
    /// serializing and deserializing when the type information is passed around, for example
    /// when pushing tasks / messages around.
    /// </remarks>
    /// <param name="type">The type to get the name from.</param>
    /// <returns>A 'simple' qualified name.</returns>
    public static string SimpleAssemblyQualifiedName(this Type type)
    {
        Guard.NotNull(nameof(type), type);

        return string.Concat(type.FullName, ", ", type.Assembly.GetName().Name);
    }

    public static IEnumerable<Type> GetTypeHierarchy(this Type type)
    {
        yield return type;

        // is there any base type?
        if ((type == null) || (type.BaseType == null))
        {
            yield break;
        }

        // return all implemented or inherited interfaces
        foreach (var i in type.GetInterfaces())
        {
            yield return i;
        }

        // return all inherited types
        var currentBaseType = type.BaseType;
        while (currentBaseType != null)
        {
            yield return currentBaseType;
            currentBaseType = currentBaseType.BaseType;
        }
    }

    public static IEnumerable<T> FindAttributeInHierarchy<T>(this Type type) where T : Attribute
    {
        return type.GetTypeHierarchy().Select(t => t.GetCustomAttribute<T>(false)).Where(a => a != null);
    }

    public static bool CanAssignNull(this Type type)
    {
        return !type.IsValueType || (Nullable.GetUnderlyingType(type) != null);
    }

    /// <summary>
    /// Checks whether this <see cref="Type" /> is a type of the given generic type.
    /// </summary>
    /// <param name="typeToCheck">The type to check (i.e. <c>List&lt;string&gt;</c>).</param>
    /// <param name="genericType">The generic type to check against (i.e. <c>List&lt;&gt;</c>).</param>
    /// <returns>Whether <paramref name="typeToCheck" /> is a type of <paramref name="genericType"/>.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="genericType" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">If <paramref name="genericType" /> is not a generic type definition.</exception>
    public static bool IsOfGenericType(this Type typeToCheck, Type genericType)
    {
        return typeToCheck.IsOfGenericType(genericType, out _);
    }

    /// <summary>
    /// Checks whether this <see cref="Type" /> is a type of the given generic type.
    /// </summary>
    /// <param name="typeToCheck">The type to check (i.e. <c>List&lt;string&gt;</c>).</param>
    /// <param name="genericType">The generic type to check against (i.e. <c>List&lt;&gt;</c>).</param>
    /// <param name="concreteGenericType">Set to the actual concrete type found.</param>
    /// <returns>Whether <paramref name="typeToCheck" /> is a type of <paramref name="genericType"/>.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="genericType" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">If <paramref name="genericType" /> is not a generic type definition.</exception>
    public static bool IsOfGenericType(this Type typeToCheck, Type genericType, out Type concreteGenericType)
    {
        if (genericType == null)
        {
            throw new ArgumentNullException(nameof(genericType));
        }

        if (!genericType.IsGenericTypeDefinition)
        {
            throw new ArgumentException("The definition needs to be a GenericTypeDefinition (i.e typeof(List<>))", nameof(genericType));
        }

        while (true)
        {
            concreteGenericType = null;

            if (typeToCheck == null || typeToCheck == typeof(object))
            {
                return false;
            }

            if (typeToCheck == genericType)
            {
                concreteGenericType = typeToCheck;
                return true;
            }

            if ((typeToCheck.IsGenericType ? typeToCheck.GetGenericTypeDefinition() : typeToCheck) == genericType)
            {
                concreteGenericType = typeToCheck;
                return true;
            }

            if (genericType.IsInterface)
            {
                foreach (var i in typeToCheck.GetInterfaces())
                {
                    if (i.IsOfGenericType(genericType, out concreteGenericType))
                    {
                        return true;
                    }
                }
            }

            typeToCheck = typeToCheck.BaseType;
        }
    }
}