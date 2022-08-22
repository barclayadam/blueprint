using System;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Blueprint.Compiler;

/// <summary>
/// A strategy for compiling code in to an assembly.
/// </summary>
/// <seealso cref="InMemoryOnlyCompileStrategy" />
/// <seealso cref="ToFileCompileStrategy" />
public interface ICompileStrategy
{
    /// <summary>
    /// Performs the actual compilation of the given <see cref="CSharpCompilation" />, returning the
    /// loaded <see cref="Assembly" /> that results form the compilation.
    /// </summary>
    /// <param name="compilation">The compilation model.</param>
    /// <param name="check">A method that should be called with the <see cref="EmitResult" /> of compilation to check for errors.</param>
    /// <returns>A loaded <see cref="Assembly" /> from the given compilation.</returns>
    Assembly Compile(CSharpCompilation compilation, Action<EmitResult> check);
}