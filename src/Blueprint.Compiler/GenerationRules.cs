using System.Collections.Generic;
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace Blueprint.Compiler
{
    public class GenerationRules
    {
        public GenerationRules(string applicationNamespace)
        {
            ApplicationNamespace = applicationNamespace;
        }

        public OptimizationLevel OptimizationLevel { get; set; } = OptimizationLevel.Release;

        public string ApplicationNamespace { get; }

        public string AssemblyName { get; set; }

        public readonly IList<Assembly> Assemblies = new List<Assembly>();
    }
}
