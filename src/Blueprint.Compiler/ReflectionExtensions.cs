using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler
{
    public static class ReflectionExtensions
    {
        private static readonly Dictionary<Type, string> _aliases = new Dictionary<Type, string>
        {
            { typeof(int), "int" },
            { typeof(void), "void" },
            { typeof(string), "string" },
            { typeof(long), "long" },
            { typeof(double), "double" },
            { typeof(bool), "bool" },
            { typeof(Task), "Task" },
            { typeof(object), "object" },
            { typeof(object[]), "object[]" },
        };

        /// <summary>
        /// Indicates whether the given method is async based on it's return type, whether it returns one of
        /// <see cref="Task" /> or <see cref="ValueTask" /> (or their generic equivalents).
        /// </summary>
        /// <param name="method">The method to check.</param>
        /// <returns>Whether it is async.</returns>
        public static bool IsAsync(this MethodInfo method)
        {
            var _ = method ?? throw new ArgumentNullException(nameof(method));

            return method.ReturnType == typeof(Task) || method.ReturnType.Closes(typeof(Task<>)) ||
                   method.ReturnType == typeof(ValueTask) || method.ReturnType.Closes(typeof(ValueTask<>));
        }

        public static bool CanBeOverridden(this MethodInfo method)
        {
            if (method.IsAbstract)
            {
                return true;
            }

            if (method.IsVirtual && !method.IsFinal)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Derives the full type name *as it would appear in C# code*.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string FullNameInCode(this Type type)
        {
            if (_aliases.ContainsKey(type))
            {
                return _aliases[type];
            }

            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                var cleanName = type.Name.Split('`').First();
                if (type.IsNested && type.DeclaringType?.IsGenericTypeDefinition == true)
                {
                    cleanName = $"{type.ReflectedType.NameInCode(type.GetGenericArguments())}.{cleanName}";
                    return $"{type.Namespace}.{cleanName}";
                }

                if (type.IsNested)
                {
                    cleanName = $"{type.ReflectedType.NameInCode()}.{cleanName}";
                }

                var tempQualifier = type.GetGenericArguments().Select(x => x.FullNameInCode());
                var args = string.Join(", ", tempQualifier);

                return $"{type.Namespace}.{cleanName}<{args}>";
            }

            if (type.FullName == null)
            {
                return type.Name;
            }

            return type.FullName.Replace("+", ".");
        }

        /// <summary>
        /// Derives the type name *as it would appear in C# code*.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string NameInCode(this Type type)
        {
            if (_aliases.ContainsKey(type))
            {
                return _aliases[type];
            }

            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                var cleanName = type.Name.Split('`').First().Replace("+", ".");
                if (type.IsNested)
                {
                    cleanName = $"{type.ReflectedType.NameInCode()}.{cleanName}";
                }

                var tempQualifier = type.GetGenericArguments().Select(x => x.FullNameInCode());
                var args = string.Join(", ", tempQualifier);

                return $"{cleanName}<{args}>";
            }

            if (type.MemberType == MemberTypes.NestedType)
            {
                return $"{type.ReflectedType.NameInCode()}.{type.Name}";
            }

            return type.Name.Replace("+", ".").Replace("`", "_");
        }

        /// <summary>
        /// Derives the type name *as it would appear in C# code* for a type with generic parameters.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericParameterTypes"></param>
        /// <returns></returns>
        public static string NameInCode(this Type type, Type[] genericParameterTypes)
        {
            var cleanName = type.Name.Split('`').First().Replace("+", ".");
            var tempQualifier = genericParameterTypes.Select(x => x.FullNameInCode());
            var args = string.Join(", ", tempQualifier);

            return $"{cleanName}<{args}>";
        }
    }
}
