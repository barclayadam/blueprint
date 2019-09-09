using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Blueprint.Compiler.Util
{
    internal static class TypeExtensions
    {
        private static readonly IList<Type> IntegerTypes = new List<Type>
        {
            typeof(byte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(sbyte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(byte?),
            typeof(short?),
            typeof(int?),
            typeof(long?),
            typeof(sbyte?),
            typeof(ushort?),
            typeof(uint?),
            typeof(ulong?)
        };

        private static readonly Type[] TupleTypes = {
            typeof(ValueTuple<>),
            typeof(ValueTuple<,>),
            typeof(ValueTuple<,,>),
            typeof(ValueTuple<,,,>),
            typeof(ValueTuple<,,,,>),
            typeof(ValueTuple<,,,,,>),
            typeof(ValueTuple<,,,,,,>),
            typeof(ValueTuple<,,,,,,,>)
        };

        public static bool CanBeCastTo<T>(this Type type)
        {
            if (type == null) return false;
            var destinationType = typeof(T);

            return CanBeCastTo(type, destinationType);
        }

        public static bool CanBeCastTo(this Type type, Type destinationType)
        {
            if (type == null) return false;
            if (type == destinationType) return true;

            return destinationType.IsAssignableFrom(type);
        }

        public static Type FindInterfaceThatCloses(this Type type, Type openType)
        {
            if (type == typeof(object)) return null;

            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsInterface && typeInfo.IsGenericType && type.GetGenericTypeDefinition() == openType)
                return type;


            foreach (var interfaceType in type.GetInterfaces())
            {
                var interfaceTypeInfo = interfaceType.GetTypeInfo();
                if (interfaceTypeInfo.IsGenericType && interfaceType.GetGenericTypeDefinition() == openType)
                {
                    return interfaceType;
                }
            }

            if (!type.IsConcrete()) return null;


            return typeInfo.BaseType == typeof(object)
                ? null
                : typeInfo.BaseType.FindInterfaceThatCloses(openType);
        }

        public static Type FindParameterTypeTo(this Type type, Type openType)
        {
            var interfaceType = type.FindInterfaceThatCloses(openType);
            return interfaceType?.GetGenericArguments().FirstOrDefault();
        }

        public static bool Closes(this Type type, Type openType)
        {
            if (type == null) return false;

            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == openType) return true;

            foreach (var @interface in type.GetInterfaces())
            {
                if (@interface.Closes(openType)) return true;
            }

            var baseType = typeInfo.BaseType;
            if (baseType == null) return false;

            var baseTypeInfo = baseType.GetTypeInfo();

            var closes = baseTypeInfo.IsGenericType && baseType.GetGenericTypeDefinition() == openType;
            if (closes) return true;

            return typeInfo.BaseType?.Closes(openType) ?? false;
        }

        public static string GetName(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType)
            {
                var parameters = type.GetGenericArguments().Select(x => x.GetName()).ToArray();
                var parameterList = string.Join(", ", parameters);
                object[] args = { type.Name, parameterList };
                return String.Format("{0}<{1}>", args);
            }

            return type.Name;
        }

        public static bool IsString(this Type type)
        {
            return type == typeof(string);
        }

        public static bool IsPrimitive(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPrimitive && !IsString(type) && type != typeof(IntPtr);
        }

        public static bool IsSimple(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPrimitive || IsString(type) || typeInfo.IsEnum;
        }

        public static bool IsConcrete(this Type type)
        {
            if (type == null) return false;

            var typeInfo = type.GetTypeInfo();

            return !typeInfo.IsAbstract && !typeInfo.IsInterface;
        }

        public static bool IsNotConcrete(this Type type)
        {
            return !type.IsConcrete();
        }

        /// <summary>
        ///     Returns true if the type is a DateTime or nullable DateTime
        /// </summary>
        /// <param name="typeToCheck"></param>
        /// <returns></returns>
        public static bool IsDateTime(this Type typeToCheck)
        {
            return typeToCheck == typeof(DateTime) || typeToCheck == typeof(DateTime?);
        }

        public static bool IsBoolean(this Type typeToCheck)
        {
            return typeToCheck == typeof(bool) || typeToCheck == typeof(bool?);
        }

        /// <summary>
        ///     Returns a boolean value indicating whether or not the type is:
        ///     int, long, decimal, short, float, or double
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Bool indicating whether the type is numeric</returns>
        public static bool IsNumeric(this Type type)
        {
            return type.IsFloatingPoint() || type.IsIntegerBased();
        }

        /// <summary>
        ///     Returns a boolean value indicating whether or not the type is:
        ///     int, long or short
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Bool indicating whether the type is integer based</returns>
        public static bool IsIntegerBased(this Type type)
        {
            return IntegerTypes.Contains(type);
        }

        /// <summary>
        ///     Returns a boolean value indicating whether or not the type is:
        ///     decimal, float or double
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Bool indicating whether the type is floating point</returns>
        public static bool IsFloatingPoint(this Type type)
        {
            return type == typeof(decimal) || type == typeof(float) || type == typeof(double);
        }

        public static T CloseAndBuildAs<T>(this Type openType, params Type[] parameterTypes)
        {
            var closedType = openType.MakeGenericType(parameterTypes);
            return (T) Activator.CreateInstance(closedType);
        }

        public static T CloseAndBuildAs<T>(this Type openType, object ctorArgument, params Type[] parameterTypes)
        {
            var closedType = openType.MakeGenericType(parameterTypes);
            return (T) Activator.CreateInstance(closedType, ctorArgument);
        }

        public static T CloseAndBuildAs<T>(this Type openType, object ctorArgument1, object ctorArgument2,
            params Type[] parameterTypes)
        {
            var closedType = openType.MakeGenericType(parameterTypes);
            return (T) Activator.CreateInstance(closedType, ctorArgument1, ctorArgument2);
        }

        public static bool PropertyMatches(this PropertyInfo prop1, PropertyInfo prop2)
        {
            return prop1.DeclaringType == prop2.DeclaringType && prop1.Name == prop2.Name;
        }

        public static T Create<T>(this Type type)
        {
            return (T) type.Create();
        }

        public static object Create(this Type type)
        {
            return Activator.CreateInstance(type);
        }

        public static Type DeriveElementType(this Type type)
        {
            return type.GetElementType() ?? type.GetGenericArguments().FirstOrDefault();
        }

        public static Type IsAnEnumerationOf(this Type type)
        {
            if (!type.Closes(typeof(IEnumerable<>)))
            {
                throw new Exception("Duh, its gotta be enumerable");
            }

            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (type.GetTypeInfo().IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }


            throw new Exception($"I don't know how to figure out what this is a collection of. Can you tell me? {type}");
        }

        public static void ForAttribute<T>(this Type type, Action<T> action) where T : Attribute
        {
            var atts = type.GetTypeInfo().GetCustomAttributes(typeof(T));
            foreach (T att in atts)
            {
                action(att);
            }
        }

        public static void ForAttribute<T>(this Type type, Action<T> action, Action elseDo)
            where T : Attribute
        {
            var atts = type.GetTypeInfo().GetCustomAttributes(typeof(T)).ToArray();
            foreach (T att in atts)
            {
                action(att);
            }

            if (!atts.Any())
            {
                elseDo();
            }
        }

        public static bool HasAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetTypeInfo().GetCustomAttributes<T>().Any();
        }

        public static T GetAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetTypeInfo().GetCustomAttributes<T>().FirstOrDefault();
        }

        public static bool IsValueTuple(this Type type)
        {
            return (type != null && type.IsGenericType) && TupleTypes.Contains(type.GetGenericTypeDefinition());
        }
    }
}
