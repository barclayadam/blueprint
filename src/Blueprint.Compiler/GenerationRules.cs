using System;

using Microsoft.CodeAnalysis;

namespace Blueprint.Compiler
{
    public class GenerationRules
    {
        public GenerationRules(string applicationNamespace)
        {
            ApplicationNamespace = applicationNamespace;
            UseCompileStrategy<ToFileCompileStrategy>();
        }

        public OptimizationLevel OptimizationLevel { get; set; } = OptimizationLevel.Release;

        public string ApplicationNamespace { get; set; }

        public string AssemblyName { get; set; }

        public Type CompileStrategy { get; private set; }

        public void UseCompileStrategy<T>() where T : ICompileStrategy
        {
            CompileStrategy = typeof(T);
        }
    }
}
