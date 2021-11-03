using System.Collections.Generic;
using Blueprint.Compiler.Model;
using Microsoft.CodeAnalysis;

namespace Blueprint.Compiler
{
    /// <summary>
    /// A set of rules and options that influence how an assembly is generated / compiled.
    /// </summary>
    public class GenerationRules
    {
        /// <summary>
        /// The Rosyln optimisation level applied when compiling the assembly.
        /// </summary>
        public OptimizationLevel OptimizationLevel { get; set; } = OptimizationLevel.Release;

        /// <summary>
        /// The name of the compiled assembly.
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Variable sources that are added to the default list of variable sources, for example
        /// HTTP-related variables if using the HTTP host.
        /// </summary>
        public List<IVariableSource> VariableSources { get; } = new List<IVariableSource>();
    }
}
