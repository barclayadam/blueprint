using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Blueprint.Compiler
{
    /// <summary>
    /// An <see cref="ICompileStrategy" /> that will generate DLLs in-memory only and never store them to
    /// disk or try to reload existing ones, primarily used for unit tests.
    /// </summary>
    public class InMemoryOnlyCompileStrategy : ICompileStrategy
    {
        /// <inheritdoc />
        public Assembly Compile(CSharpCompilation compilation, Action<EmitResult> check)
        {
            using var assemblyStream = new MemoryStream();
            using var symbolsStream = new MemoryStream();

            var emitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb);
            var embeddedTexts = compilation.SyntaxTrees.Select(s => EmbeddedText.FromSource(s.FilePath, s.GetText()));

            var result = compilation.Emit(
                peStream: assemblyStream,
                pdbStream: symbolsStream,
                embeddedTexts: embeddedTexts,
                options: emitOptions);

            check(result);

            assemblyStream.Seek(0, SeekOrigin.Begin);
            symbolsStream.Seek(0, SeekOrigin.Begin);

            return AssemblyLoadContext.Default.LoadFromStream(assemblyStream, symbolsStream);
        }
    }
}
