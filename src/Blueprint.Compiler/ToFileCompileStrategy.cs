using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;

namespace Blueprint.Compiler
{
    /// <summary>
    /// An <see cref="ICompileStrategy" /> that will store generated DLLs and symbol files to disk
    /// and use an associated manifest file to allow for the re-use of generated DLLs.
    /// </summary>
    public class ToFileCompileStrategy : ICompileStrategy
    {
        private readonly ILogger<ToFileCompileStrategy> logger;
        private readonly string outputFolder;

        /// <summary>
        /// Initialises a new instanced of the <see cref="ToFileCompileStrategy" />.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="outputFolder">The folder that we should load / save generated DLLs.</param>
        public ToFileCompileStrategy(ILogger<ToFileCompileStrategy> logger, string outputFolder)
        {
            this.logger = logger;
            this.outputFolder = outputFolder;
        }

        /// <summary>
        /// Tries to load an existing Assembly using the given source text hash and assembly name, used as an
        /// optimisation to avoid any actual compilation if possible.
        /// </summary>
        /// <param name="sourceTextHash">A hash of the source that would be compiled to an assembly.</param>
        /// <param name="assemblyName">The name of the assembly (including the .dll extension).</param>
        /// <returns>A loaded <see cref="Assembly" /> if a matching one already exists, or <c>null</c> if not.</returns>
        public Assembly TryLoadExisting(string sourceTextHash, string assemblyName)
        {
            var manifestName = Path.ChangeExtension(assemblyName, "manifest");

            // Search through the potential paths for a candidate DLL to load. If we find one we load and return
            // from the foreach, otherwise we know it need to be compiled (see below loop).
            var assemblyFile = Path.Combine(outputFolder, assemblyName);
            var manifestFile = Path.Combine(outputFolder, manifestName);

            // If we have a previously generated DLL _and_ the manifest we have saved is the same hash
            // as we have now we can skip the actual compilation and re-use the existing DLL.
            //
            // This enables a quick restart of an API if required, such as normal recycling of processes
            // etc. and can also help local development as the source would only change with an API operation
            // definition change, not _any_ code change in the solution
            if (File.Exists(assemblyFile) && File.Exists(manifestFile))
            {
                var previousHash = File.ReadAllText(manifestFile);

                if (previousHash == sourceTextHash)
                {
                    logger.LogInformation(
                        "NOT compiling as previous compilation exists at '{AssemblyFile}'. Hash of compilation is '{SourceTextHash}'",
                        assemblyFile,
                        sourceTextHash);

                    try
                    {
                        return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Unable to load existing DLL '{AssemblyFile}", assemblyFile);
                    }
                }
                else
                {
                    logger.LogInformation(
                        "NOT using assembly at {AssemblyFile} because it's manifest {PreviousHash} does NOT match required {SourceTextHash}",
                        assemblyFile,
                        previousHash,
                        sourceTextHash);
                }
            }
            else
            {
                logger.LogInformation("No previously generated DLL exists at '{AssemblyFile}'", assemblyFile);
            }

            return null;
        }

        /// <summary>
        /// Performs the actual compilation of the given <see cref="CSharpCompilation" /> and stores / loads the resulting
        /// assembly, including potentially some form of manifest that can be used by <see cref="TryLoadExisting" /> to load
        /// the DLL in subsequent runs without performing any compilation.
        /// </summary>
        /// <param name="sourceTextHash">A hash of the source that would be compiled to an assembly.</param>
        /// <param name="compilation">The compilation model.</param>
        /// <param name="check">A method that should be called with the <see cref="EmitResult" /> of compilation to check for errors.</param>
        /// <returns>A loaded <see cref="Assembly" /> from the given compilation.</returns>
        public Assembly Compile(string sourceTextHash, CSharpCompilation compilation, Action<EmitResult> check)
        {
            var assemblyName = compilation.AssemblyName;
            var symbolsName = Path.ChangeExtension(assemblyName, "pdb");
            var manifestName = Path.ChangeExtension(assemblyName, "manifest");

            // Search through the potential paths for a candidate DLL to load. If we find one we load and return
            // from the foreach, otherwise we know it need to be compiled (see below loop).
            var assemblyFile = Path.Combine(outputFolder, assemblyName);
            var manifestFile = Path.Combine(outputFolder, manifestName);

            // Search through the potential paths for a candidate DLL to load. If we find one we load and return
            // from the foreach, otherwise we know it need to be compiled (see below loop).
            var symbolsFile = Path.Combine(outputFolder, symbolsName);

            try
            {
                logger.LogInformation("Compiling source to DLL at '{0}'. Hash of compilation is '{1}'", assemblyFile, sourceTextHash);

                Compile(compilation, check, compilation.SyntaxTrees, assemblyFile, symbolsFile);

                File.WriteAllText(manifestFile, sourceTextHash);

                return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
            }
            catch (IOException e)
            {
                logger.LogCritical(e, "Could not compile to {AssemblyName}", assemblyFile);

                throw;
            }
        }

        private void Compile(
            CSharpCompilation compilation,
            Action<EmitResult> check,
            IEnumerable<SyntaxTree> syntaxTrees,
            string assemblyFile,
            string symbolsFile)
        {
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
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

            logger.LogInformation(
                $"Compiled assembly to \"{assemblyFile}\" ({new FileInfo(assemblyFile).Length} bytes) and " +
                $"symbols to \"{symbolsFile}\" ({new FileInfo(symbolsFile).Length} bytes)");
        }
    }
}
