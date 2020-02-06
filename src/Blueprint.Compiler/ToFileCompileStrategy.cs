using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;

#if !NET472
using System.Runtime.Loader;
#endif

namespace Blueprint.Compiler
{
    public class ToFileCompileStrategy : ICompileStrategy
    {
        private readonly ILogger<ToFileCompileStrategy> logger;
        private readonly string outputFolder;

        public ToFileCompileStrategy(ILogger<ToFileCompileStrategy> logger, string outputFolder)
        {
            this.logger = logger;
            this.outputFolder = outputFolder;
        }

        public Assembly Compile(CSharpCompilation compilation, Action<EmitResult> check)
        {
            var sourceTexts = compilation.SyntaxTrees.Select(s => s.GetText());
            var sourceTextHash = GetSha256Hash(string.Join("\n\n", sourceTexts));

            var assemblyName = compilation.AssemblyName + ".dll";
            var symbolsName = Path.ChangeExtension(assemblyName, "pdb");
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
#if !NET472
                        return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
#else
                        return Assembly.LoadFrom(assemblyFile);
#endif
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

            // Search through the potential paths for a candidate DLL to load. If we find one we load and return
            // from the foreach, otherwise we know it need to be compiled (see below loop).
            var symbolsFile = Path.Combine(outputFolder, symbolsName);

            try
            {
                logger.LogInformation("Compiling source to DLL at '{0}'. Hash of compilation is '{1}'", assemblyFile, sourceTextHash);

                Compile(compilation, check, compilation.SyntaxTrees, assemblyFile, symbolsFile);

                File.WriteAllText(manifestFile, sourceTextHash);

#if !NET472
                return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
#else
                return Assembly.LoadFrom(assemblyFile);
#endif
            }
            catch (IOException e)
            {
                logger.LogCritical(e, "Could not compile to {AssemblyName}", assemblyFile);

                throw;
            }
        }

        private static string GetSha256Hash(string input)
        {
            using (var shaHash = SHA256.Create())
            {
                var data = shaHash.ComputeHash(Encoding.UTF8.GetBytes(input));

                var sBuilder = new StringBuilder();

                foreach (var t in data)
                {
                    sBuilder.Append(t.ToString("x2"));
                }

                return sBuilder.ToString();
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

            using (var assemblyStream = File.OpenWrite(assemblyFile))
            using (var symbolsStream = File.OpenWrite(symbolsFile))
            {
                var emitOptions = new EmitOptions(
                    debugInformationFormat: DebugInformationFormat.PortablePdb);

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
}
