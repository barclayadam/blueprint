using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;
using System.Runtime.Loader;

namespace Blueprint.Compiler
{
    /// <summary>
    /// An <see cref="ICompileStrategy" /> that will generate DLLs in-memory only and never store them to
    /// disk or try to reload existing ones, primarily used for unit tests.
    /// </summary>
    public class InMemoryOnlyCompileStrategy : ICompileStrategy
    {
        private readonly ILogger<InMemoryOnlyCompileStrategy> _logger;

        /// <summary>
        /// Initialises a new instance of the <see cref="InMemoryOnlyCompileStrategy" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public InMemoryOnlyCompileStrategy(ILogger<InMemoryOnlyCompileStrategy> logger)
        {
            this._logger = logger;
        }

        /// <inheritdoc />
        public Assembly TryLoadExisting(string sourceTextHash, string assemblyName)
        {
            // We never try to load an existing in-memory DLL.
            return null;
        }

        /// <inheritdoc />
        public Assembly Compile(string sourceTextHash, CSharpCompilation compilation, Action<EmitResult> check)
        {
            this._logger.LogInformation("Compiling source to an in-memory DLL with embedded source");

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
