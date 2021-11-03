using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Blueprint.Compiler.Util
{
    /// <summary>
    /// A set of reflection-specific extension methods.
    /// </summary>
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
        /// Gets the <see cref="PropertyInfo" /> that a lambda refers to, allowing compile-time safe(r) access to properties
        /// given a known type.
        /// </summary>
        /// <param name="propertyExpression">A lambda that accesses a property from <typeparamref name="TSource" />.</param>
        /// <typeparam name="TSource">The source type from which a property should be grabbed.</typeparam>
        /// <typeparam name="TProperty">The type of the property that will be returned.</typeparam>
        /// <returns>A <see cref="PropertyInfo" /> the lambda represents.</returns>
        /// <exception cref="ArgumentException">If the lambda does not refer to a property.</exception>
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyExpression)
        {
            var type = typeof(TSource);

            if (propertyExpression.Body is not MemberExpression member)
            {
                throw new ArgumentException($"Expression '{propertyExpression}' refers to a method, not a property.");
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException($"Expression '{propertyExpression}' refers to a field, not a property.");
            }

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException($"Expression '{propertyExpression}' refers to a property that is not from type {type}.");
            }

            return propInfo;
        }

        public static Type DetermineElementType(this Type serviceType)
        {
            if (serviceType.IsArray)
            {
                return serviceType.GetElementType();
            }

            return serviceType.GetGenericArguments().First();
        }

        /// <summary>
        /// Indicates whether the given method is async based on it's return type, whether it returns one of
        /// <see cref="Task" /> or <see cref="ValueTask" /> (or their generic equivalents).
        /// </summary>
        /// <param name="method">The method to check.</param>
        /// <returns>Whether it is async.</returns>
        // ReSharper disable once InconsistentNaming
        public static bool IsAsync(this MethodInfo method)
        {
            var _ = method ?? throw new ArgumentNullException(nameof(method));

            return method.ReturnType == typeof(Task) || method.ReturnType.Closes(typeof(Task<>)) ||
                   method.ReturnType == typeof(ValueTask) || method.ReturnType.Closes(typeof(ValueTask<>));
        }

        /// <summary>
        /// Determines if it is possible for this method to be overriden in a subclass (i.e. if it is
        /// <c>abstract</c>, or <c>virtual</c> and NOT <c>final</c>.
        /// </summary>
        /// <param name="method">The method to check.</param>
        /// <returns>Whether the method can be overriden.</returns>
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
        /// Derives the full type name (i.e. including namespace) *as it would appear in C# code*.
        /// </summary>
        /// <param name="type">The type to generate a name for.</param>
        /// <returns>C# version of the type, as a name.</returns>
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
        /// Derives the type name *as it would appear in C# code* (i.e. without namespace).
        /// </summary>
        /// <param name="type">The type to generate a name for.</param>
        /// <returns>C# version of the type, as a name.</returns>
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

        private static string NameInCode(this Type type, Type[] genericParameterTypes)
        {
            var cleanName = type.Name.Split('`').First().Replace("+", ".");
            var tempQualifier = genericParameterTypes.Select(x => x.FullNameInCode());
            var args = string.Join(", ", tempQualifier);

            return $"{cleanName}<{args}>";
        }
    }
}
