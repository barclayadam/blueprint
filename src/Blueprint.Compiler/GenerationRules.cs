using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Blueprint.Compiler
{
    public class GenerationRules
    {
        public GenerationRules(string applicationNamespace)
        {
            ApplicationNamespace = applicationNamespace;
            AssemblyName = Assembly.GetExecutingAssembly().GetName().Name + "Generated";
        }

        public OptimizationLevel OptimizationLevel { get; set; } = OptimizationLevel.Release;

        public string ApplicationNamespace { get; set; }

        public string AssemblyName { get; set; }
    }
}
