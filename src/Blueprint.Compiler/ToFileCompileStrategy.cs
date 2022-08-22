using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    /// An <see cref="ICompileStrategy" /> that will store generated DLLs and symbol files to disk.
    /// </summary>
    public class ToFileCompileStrategy : ICompileStrategy
    {
        private readonly string _outputFolder;

        /// <summary>
        /// Initialises a new instanced of the <see cref="ToFileCompileStrategy" />.
        /// </summary>
        /// <param name="outputFolder">The folder that we should load / save generated DLLs.</param>
        public ToFileCompileStrategy(string outputFolder)
        {
            this._outputFolder = outputFolder;
        }

        /// <summary>
        /// Performs the actual compilation of the given <see cref="CSharpCompilation" />, outputting to an assembly and PDB
        /// file in the configured output folder and loading the assembly in to memory.
        /// </summary>
        /// <param name="compilation">The compilation model.</param>
        /// <param name="check">A method that should be called with the <see cref="EmitResult" /> of compilation to check for errors.</param>
        /// <returns>A loaded <see cref="Assembly" /> from the given compilation.</returns>
        public Assembly Compile(CSharpCompilation compilation, Action<EmitResult> check)
        {
            var assemblyName = compilation.AssemblyName;

            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                throw new InvalidOperationException(
                    $"Cannot use a {nameof(ToFileCompileStrategy)} without setting the assembly name of the compilation");
            }

            var symbolsName = Path.ChangeExtension(assemblyName, "pdb");

            // Search through the potential paths for a candidate DLL to load. If we find one we load and return
            // from the foreach, otherwise we know it need to be compiled (see below loop).
            var assemblyFile = Path.Combine(this._outputFolder, assemblyName);

            // Search through the potential paths for a candidate DLL to load. If we find one we load and return
            // from the foreach, otherwise we know it need to be compiled (see below loop).
            var symbolsFile = Path.Combine(this._outputFolder, symbolsName);

            this.Compile(compilation, check, compilation.SyntaxTrees, assemblyFile, symbolsFile);

            return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
        }

        private void Compile(
            CSharpCompilation compilation,
            Action<EmitResult> check,
            ImmutableArray<SyntaxTree> syntaxTrees,
            string assemblyFile,
            string symbolsFile)
        {
            if (!Directory.Exists(this._outputFolder))
            {
                Directory.CreateDirectory(this._outputFolder);
            }

            using var assemblyStream = File.OpenWrite(assemblyFile);
            using var symbolsStream = File.OpenWrite(symbolsFile);

            var emitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb);

            var embeddedTexts = syntaxTrees.Select(s => EmbeddedText.FromSource(s.FilePath, s.GetText()));

            var result = compilation.Emit(
                peStream: assemblyStream,
                pdbStream: symbolsStream,
                embeddedTexts: embeddedTexts,
                options: emitOptions);

            check(result);
        }
    }
}
