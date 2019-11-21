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
        private static readonly string[] PotentialPaths =
        {
            Path.GetDirectoryName(typeof(ToFileCompileStrategy).Assembly.Location),
            Path.Combine(Path.GetTempPath(), "Blueprint.Compiler"),
        };

        private readonly ILogger<ToFileCompileStrategy> logger;

        public ToFileCompileStrategy(ILogger<ToFileCompileStrategy> logger)
        {
            this.logger = logger;
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
            foreach (var outputFolder in PotentialPaths)
            {
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
                        logger.LogInformation("NOT compiling as previous compilation exists at '{0}'. Hash of compilation is '{1}'", assemblyFile, sourceTextHash);

#if !NET472
                        return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
#else
                        return Assembly.LoadFrom(assemblyFile);
#endif
                    }
                }
            }

            var ioExceptions = new Dictionary<string, Exception>();

            // Search through the potential paths for a candidate DLL to load. If we find one we load and return
            // from the foreach, otherwise we know it need to be compiled (see below loop).
            foreach (var outputFolder in PotentialPaths)
            {
                var assemblyFile = Path.Combine(outputFolder, assemblyName);
                var symbolsFile = Path.Combine(outputFolder, symbolsName);
                var manifestFile = Path.Combine(outputFolder, manifestName);

                try
                {
                    logger.LogInformation("Compiling source to DLL at '{0}'. Hash of compilation is '{1}'", assemblyFile, sourceTextHash);

                    Compile(compilation, check, compilation.SyntaxTrees, outputFolder, assemblyFile, symbolsFile);

                    File.WriteAllText(manifestFile, sourceTextHash);

#if !NET472
                    return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
#else
                    return Assembly.LoadFrom(assemblyFile);
#endif
                }
                catch (IOException e)
                {
                    logger.LogWarning(e, "Could not compile to directory '{0}'.", outputFolder);

                    ioExceptions[outputFolder] = e;

                    // We cannot, for whatever reason, write to this output folder so try another one. At this point the exception is
                    // not considered harmful, only if we exit the loop without being able to write anywhere
                    continue;
                }
            }

            var attempted = string.Join("\n", ioExceptions.Select(k => $"    {k.Key}: {k.Value.Message}"));

            throw new InvalidOperationException(
                $"Failed to write to any candidate path when compiling. Check the below attempted paths and their exception messages and either " +
                $"fix the issues to enable writing to one of the paths, or switch to using the in memory compile strategy.:\n\n{attempted}");
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
            string outputFolder,
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
