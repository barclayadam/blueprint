namespace Blueprint.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    using Core.Utilities;

    using Shouldly;

    using StructureMap.TypeRules;

    public class NoAsyncVoid
    {
        public static void Check(Assembly assembly, params Type[] typesToExclude)
        {
            var messages = GetAsyncVoidMethods(assembly, typesToExclude)
                .Select(method => $"'{GetTopDeclaringType(method.DeclaringType).Name}.{method.Name}' is an async void method.")
                .ToList();

            messages.Any().ShouldBeFalse("Async void methods found!" + Environment.NewLine + string.Join(Environment.NewLine, messages));
        }

        public static IEnumerable<MethodInfo> GetAsyncVoidMethods(Assembly assembly, params Type[] typesToExclude)
        {
            return assembly.GetLoadableTypes()
                .SelectMany(type => type.GetMethods(
                    BindingFlags.NonPublic
                    | BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly))
                .Where(method => !typesToExclude.Contains(GetTopDeclaringType(method.DeclaringType)))
                .Where(method => method.HasAttribute<AsyncStateMachineAttribute>())
                .Where(method => method.ReturnType == typeof(void));
        }

        private static Type GetTopDeclaringType(Type type)
        {
            while (true)
            {
                if (type.DeclaringType == null)
                {
                    return type;
                }

                type = type.DeclaringType;
            }
        }
    }
}