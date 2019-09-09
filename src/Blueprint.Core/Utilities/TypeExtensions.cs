using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Blueprint.Core.Utilities
{
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
        /// <returns>A 'simple' qualified name</returns>
        public static string SimpleAssemblyQualifedName(this Type type)
        {
            Guard.NotNull(nameof(type), type);

            return String.Concat(type.FullName, ", ", type.Assembly.GetName().Name);
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
    }
}