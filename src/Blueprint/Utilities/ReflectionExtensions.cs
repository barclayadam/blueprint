﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Blueprint.Utilities;

/// <summary>
/// Provides a number of extension methods to reflection-based classes, consisting of small
/// utility methods that can cut down on a small amount of boilerplate elsewhere in the codebase.
/// </summary>
public static class ReflectionExtensions
{
    /// <summary>
    /// Indicates whether or not the specified type is a nullable type or not.
    /// </summary>
    /// <param name="type">The type which is to be checked.</param>
    /// <returns>Whether or not the specified type is a Nullable type.</returns>
    public static bool CanBeNull(this Type type)
    {
        Guard.NotNull(nameof(type), type);

        return !type.IsPrimitive;
    }

    /// <summary>
    /// Gets attributes of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of attribute to look for.</typeparam>
    /// <param name="memberInfo">The member to search for attributes.</param>
    /// <param name="inherit"><c>true</c> to search this member's inheritance chain to find the attributes; otherwise, <c>false</c>.
    /// This parameter is ignored for properties and events.</param>
    /// <returns>The list of attributes for this member that are of the given type.</returns>
    public static IEnumerable<T> GetAttributes<T>(this MemberInfo memberInfo, bool inherit)
    {
        return memberInfo.GetCustomAttributes(typeof(T), inherit).Cast<T>();
    }

    /// <summary>
    /// Gets all attributes, including those of parent classes and interfaces that are of
    /// the specified type.
    /// </summary>
    /// <typeparam name="T">The type of attribute to look for.</typeparam>
    /// <param name="type">The type to search for attributes.</param>
    /// <returns>The list of attributes for this member that are of the given type.</returns>
    public static IEnumerable<T> GetAttributesIncludingInterface<T>(this Type type) where T : Attribute
    {
        return type.GetTypeHierarchy()
            .SelectMany(t => t.GetAttributes<T>(false))
            .ToList();
    }

    /// <summary>
    /// Given a type will get its non-nullable version of it represents a nullable type (e.g. Nullable&lt;int&gt;),
    /// or the type if it is already a non-nullable type.
    /// </summary>
    /// <param name="type">The type to get a non-nullable version from.</param>
    /// <returns>The non-nullable version of the given type.</returns>
    public static Type GetNonNullableType(this Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return Nullable.GetUnderlyingType(type);
        }

        return type;
    }

    /// <summary>
    /// Indicates whether the given member has any attributes of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of attribute to look for.</typeparam>
    /// <param name="memberInfo">The member to search for attributes.</param>
    /// <param name="inherit"><c>true</c> to search this member's inheritance chain to find the attributes; otherwise, <c>false</c>.
    /// This parameter is ignored for properties and events.</param>
    /// <returns>Whether this member has any attributes of the specified type.</returns>
    public static bool HasAttribute<T>(this MemberInfo memberInfo, bool inherit)
    {
        return memberInfo.HasAttribute(typeof(T), inherit);
    }

    /// <summary>
    /// Indicates whether the given member has any attributes of the specified type.
    /// </summary>
    /// <param name="memberInfo">The member to search for attributes.</param>
    /// <param name="attributeType">The type of attribute to look for.</param>
    /// <param name="inherit"><c>true</c> to search this member's inheritance chain to find the attributes; otherwise, <c>false</c>.
    /// This parameter is ignored for properties and events.</param>
    /// <returns>Whether this member has any attributes of the specified type.</returns>
    public static bool HasAttribute(this MemberInfo memberInfo, Type attributeType, bool inherit)
    {
        Guard.NotNull(nameof(attributeType), attributeType);

        return memberInfo.GetCustomAttributes(attributeType, inherit).Any();
    }

    /// <summary>
    /// Indicates whether or not the specified type is an IEnumerable type, such as a List
    /// or Set, excluding the string type which would typically not be treated as an actual
    /// enumerable type.
    /// </summary>
    /// <param name="type">The type which is to be checked.</param>
    /// <returns>Whether or not the specified type is an IEnumerable type.</returns>
    public static bool IsEnumerable(this Type type)
    {
        Guard.NotNull(nameof(type), type);

        return type != typeof(string) && type.GetInterface("IEnumerable") != null;
    }

    /// <summary>
    /// Gets the <see cref="CustomAttributeData" /> of an attribute that has been applied to
    /// this <see cref="MemberInfo" />, or <c>null</c> if no such attribute exists.
    /// </summary>
    /// <param name="memberInfo">The member.</param>
    /// <param name="type">The type of the attribute to search for.</param>
    /// <returns>The <see cref="CustomAttributeData" /> or <c>null</c>.</returns>
    public static CustomAttributeData GetCustomAttributeData(this MemberInfo memberInfo, Type type)
    {
        Guard.NotNull(nameof(type), type);

        return memberInfo.CustomAttributes.FirstOrDefault(a => type.IsAssignableFrom(a.AttributeType));
    }

    /// <summary>
    /// Gets the <see cref="CustomAttributeData" /> of an attribute that has been applied to
    /// this <see cref="ParameterInfo" />, or <c>null</c> if no such attribute exists.
    /// </summary>
    /// <param name="parameterInfo">The parameter.</param>
    /// <param name="type">The type of the attribute to search for.</param>
    /// <returns>The <see cref="CustomAttributeData" /> or <c>null</c>.</returns>
    public static CustomAttributeData GetCustomAttributeData(this ParameterInfo parameterInfo, Type type)
    {
        Guard.NotNull(nameof(type), type);

        return parameterInfo.CustomAttributes.FirstOrDefault(a => type.IsAssignableFrom(a.AttributeType));
    }

    /// <summary>
    /// Tries to find the constructor argument to an attribute based on it's index.
    /// </summary>
    /// <param name="customAttributeData">The attribute data to check.</param>
    /// <param name="index">The 0-based index of the argument to load.</param>
    /// <typeparam name="TValue">The expected type of the argument.</typeparam>
    /// <returns>The value used in the constructor.</returns>
    public static TValue GetConstructorArgument<TValue>(this CustomAttributeData customAttributeData, int index)
    {
        return index < customAttributeData.ConstructorArguments.Count ? (TValue)customAttributeData.ConstructorArguments[index].Value : default;
    }

    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null);
        }
    }
}