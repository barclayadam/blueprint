using System;
using System.Linq;
using Blueprint.Compiler.Tests.Codegen;
using Microsoft.Extensions.Logging.Abstractions;

namespace Blueprint.Compiler.Tests.Scenarios
{
    /// <summary>
    /// Helper class to quickly exercise and test out custom Frame classes
    /// </summary>
    public static class CodegenScenario
    {
        public static CodegenResult<TObject> ForBaseOf<TObject>(Action<GeneratedMethod> configuration)
        {
            return ForBaseOf<TObject>((t, m) => configuration(m));
        }

        public static CodegenResult<IBuilds<T>> ForBuilds<T>(Action<GeneratedMethod> configuration)
        {
            return ForBaseOf<IBuilds<T>>((t, m) => configuration(m));
        }

        public static CodegenResult<IAction<T>> ForAction<T>(Action<GeneratedType, GeneratedMethod> configuration)
        {
            return ForBaseOf<IAction<T>>(configuration);
        }

        public static CodegenResult<IAction<T1, T2>> ForAction<T1, T2>(Action<GeneratedType, GeneratedMethod> configuration)
        {
            return ForBaseOf<IAction<T1, T2>>(configuration);
        }

        public static CodegenResult<IReturningAction<TResult, T1>> ForBuilds<TResult, T1>(Action<GeneratedType, GeneratedMethod> configuration)
        {
            return ForBaseOf<IReturningAction<TResult, T1>>(configuration);
        }

        public static CodegenResult<IAction<T>> ForAction<T>(Action<GeneratedMethod> configuration)
        {
            return ForBaseOf<IAction<T>>((t, m) => configuration(m));
        }

        public static CodegenResult<IReturningAction<TResult, T1>> ForBuilds<TResult, T1>(Action<GeneratedMethod> configuration)
        {
            return ForBaseOf<IReturningAction<TResult, T1>>((t, m) => configuration(m));
        }

        public static CodegenResult<IReturningAction<TResult, T1, T2>> ForBuilds<TResult, T1, T2>(Action<GeneratedMethod> configuration)
        {
            return ForBaseOf<IReturningAction<TResult, T1, T2>>((t, m) => configuration(m));
        }

        public static CodegenResult<IReturningAction<TResult, T1, T2, T3>> ForBuilds<TResult, T1, T2, T3>(Action<GeneratedMethod> configuration)
        {
            return ForBaseOf<IReturningAction<TResult, T1, T2, T3>>((t, m) => configuration(m));
        }

        /// <summary>
        /// Generate a new method for the basic base class. The base class "TObject" should
        /// only have a single declared method
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="rules"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        private static CodegenResult<TObject> ForBaseOf<TObject>(Action<GeneratedType, GeneratedMethod> configuration)
        {
            if (typeof(TObject).GetMethods().Length != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(TObject), "The supplied base type or interface can only have exactly one declared method");
            }

            var rules = Builder.Rules();
            var assembly = new GeneratedAssembly(rules);

            var generatedType = assembly.AddType("Tests", "GeneratedType", typeof(TObject));

            var method = generatedType.Methods.Single();

            configuration(generatedType, method);

            if (typeof(TObject).IsGenericType)
            {
                foreach (var genericTypeArgument in typeof(TObject).GenericTypeArguments)
                {
                    assembly.ReferenceAssembly(genericTypeArgument.Assembly);
                }
            }

            assembly.CompileAll(new AssemblyGenerator(new InMemoryOnlyCompileStrategy()));

            return new CodegenResult<TObject>(generatedType.CreateInstance<TObject>(), generatedType.GeneratedSourceCode);
        }
    }
}
