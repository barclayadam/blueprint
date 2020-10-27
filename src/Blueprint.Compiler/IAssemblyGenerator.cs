using System.Reflection;

namespace Blueprint.Compiler
{
    public interface IAssemblyGenerator
    {
        /// <summary>
        /// Tells Roslyn to reference the given assembly and any of its dependencies
        /// when compiling code.
        /// </summary>
        /// <param name="assembly">The assembly to reference.</param>
        void ReferenceAssembly(Assembly assembly);

        void AddFile(string fileName, string code);

        /// <summary>
        /// Compile the code passed into this method to a new assembly which is loaded in to the current application.
        /// </summary>
        /// <param name="rules">Rules that are used to control the generation of the <see cref="Assembly"/>.</param>
        /// <returns>A newly constructed (and loaded) Assembly based on registered source files and given generation rules.</returns>
        Assembly Generate(GenerationRules rules);
    }
}
