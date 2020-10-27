using System;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Blueprint.Compiler
{
    public interface ICompileStrategy
    {
        /// <summary>
        /// Tries to load an existing Assembly using the given source text hash and assembly name, used as an
        /// optimisation to avoid any actual compilation if possible.
        /// </summary>
        /// <param name="sourceTextHash">A hash of the source that would be compiled to an assembly.</param>
        /// <param name="assemblyName">The name of the assembly (including the .dll extension).</param>
        /// <returns>A loaded <see cref="Assembly" /> if a matching one already exists, or <c>null</c> if not.</returns>
        Assembly TryLoadExisting(string sourceTextHash, string assemblyName);

        /// <summary>
        /// Performs the actual compilation of the given <see cref="CSharpCompilation" /> and stores / loads the resulting
        /// assembly, including potentially some form of manifest that can be used by <see cref="ToFileCompileStrategy.TryLoadExisting" /> to load
        /// the DLL in subsequent runs without performing any compilation.
        /// </summary>
        /// <param name="sourceTextHash">A hash of the source that would be compiled to an assembly.</param>
        /// <param name="compilation">The compilation model.</param>
        /// <param name="check">A method that should be called with the <see cref="EmitResult" /> of compilation to check for errors.</param>
        /// <returns>A loaded <see cref="Assembly" /> from the given compilation.</returns>
        Assembly Compile(string sourceTextHash, CSharpCompilation compilation, Action<EmitResult> check);
    }
}
