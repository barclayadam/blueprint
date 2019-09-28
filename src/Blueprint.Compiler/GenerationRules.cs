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

        public string ApplicationNamespace { get; set; }

        public string AssemblyName { get; set; }

        public IList<Assembly> Assemblies { get; } = new List<Assembly>();

        public ICompileStrategy CompileStrategy { get; set; } = new ToFileCompileStrategy();
    }
}
