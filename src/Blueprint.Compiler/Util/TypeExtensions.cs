using System;
using System.Linq;
using System.Reflection;

namespace Blueprint.Compiler.Util
{
    public static class TypeExtensions
    {
        private static readonly Type[] _tupleTypes =
        {
            typeof(ValueTuple<>),
            typeof(ValueTuple<,>),
            typeof(ValueTuple<,,>),
            typeof(ValueTuple<,,,>),
            typeof(ValueTuple<,,,,>),
            typeof(ValueTuple<,,,,,>),
            typeof(ValueTuple<,,,,,,>),
            typeof(ValueTuple<,,,,,,,>),
        };

        public static bool CanBeCastTo<T>(this Type type)
        {
            if (type == null)
            {
                return false;
            }

            var destinationType = typeof(T);

            return CanBeCastTo(type, destinationType);
        }

        public static bool CanBeCastTo(this Type type, Type destinationType)
        {
            if (type == null)
            {
                return false;
            }

            if (type == destinationType)
            {
                return true;
            }

            return destinationType.IsAssignableFrom(type);
        }

        public static bool Closes(this Type type, Type openType)
        {
            if (type == null)
            {
                return false;
            }

            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == openType)
            {
                return true;
            }

            foreach (var @interface in type.GetInterfaces())
            {
                if (@interface.Closes(openType))
                {
                    return true;
                }
            }

            var baseType = typeInfo.BaseType;
            if (baseType == null)
            {
                return false;
            }

            var baseTypeInfo = baseType.GetTypeInfo();

            var closes = baseTypeInfo.IsGenericType && baseType.GetGenericTypeDefinition() == openType;
            if (closes)
            {
                return true;
            }

            return typeInfo.BaseType?.Closes(openType) ?? false;
        }

        public static bool IsSimple(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPrimitive || IsString(type) || typeInfo.IsEnum;
        }

        public static bool IsValueTuple(this Type type)
        {
            return (type != null && type.IsGenericType) && _tupleTypes.Contains(type.GetGenericTypeDefinition());
        }

        private static bool IsString(this Type type)
        {
            return type == typeof(string);
        }
    }
}
