using System;
using System.Collections.Generic;
using System.Linq;

namespace Blueprint.Compiler.Util
{
    internal static class ReflectionExtensions
    {
        private static readonly List<Type> EnumerableTypes = new List<Type>
        {
            typeof (IEnumerable<>),
            typeof (IList<>),
            typeof (IReadOnlyList<>),
            typeof (List<>)
        };

        public static bool IsEnumerable(this Type type)
        {
            if (type.IsArray) return true;

            return type.IsGenericType && EnumerableTypes.Contains(type.GetGenericTypeDefinition());
        }

        public static Type DetermineElementType(this Type serviceType)
        {
            if (serviceType.IsArray)
            {
                return serviceType.GetElementType();
            }

            return serviceType.GetGenericArguments().First();
        }
    }
}
