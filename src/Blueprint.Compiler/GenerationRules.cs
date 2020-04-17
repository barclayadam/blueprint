using System.Collections.Generic;
using Blueprint.Compiler.Model;
using Microsoft.CodeAnalysis;

namespace Blueprint.Compiler
{
    public class GenerationRules
    {
        public OptimizationLevel OptimizationLevel { get; set; } = OptimizationLevel.Release;

        public string AssemblyName { get; set; }

        /// <summary>
        /// Variable sources that are added to the default list of variable sources, for example
        /// HTTP-related variables if using the HTTP host.
        /// </summary>
        public List<IVariableSource> VariableSources { get; } = new List<IVariableSource>();
    }
}
