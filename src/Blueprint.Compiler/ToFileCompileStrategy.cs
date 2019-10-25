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
        public Assembly Compile(ILogger logger, CSharpCompilation compilation, Action<EmitResult> check)
        {
            var sourceTexts = compilation.SyntaxTrees.Select(s => s.GetText());
            var sourceTextHash = GetSha256Hash(string.Join("\n\n", sourceTexts));

            var assemblyName = compilation.AssemblyName + ".dll";
            var symbolsName = Path.ChangeExtension(assemblyName, "pdb");
            var manifestName = Path.ChangeExtension(assemblyName, "manifest");

            var outputFolder = Path.Combine(Path.GetTempPath(), "Blueprint.Compiler");
            var assemblyFile = Path.Combine(outputFolder, assemblyName);
            var symbolsFile = Path.Combine(outputFolder, symbolsName);
            var manifestFile = Path.Combine(outputFolder, manifestName);

            var needsCompile = true;

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

                    needsCompile = false;
                }
            }

            if (needsCompile)
            {
                logger.LogInformation("Compiling source to DLL at '{0}'. Hash of compilation is '{1}'", assemblyFile, sourceTextHash);

                Compile(logger, compilation, check, compilation.SyntaxTrees, outputFolder, assemblyFile, symbolsFile);

                File.WriteAllText(manifestFile, sourceTextHash);
            }

#if !NET472
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
#else
            return Assembly.LoadFrom(assemblyFile);
#endif
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
            ILogger logger,
            CSharpCompilation compilation,
            Action<EmitResult> check,
            IEnumerable<SyntaxTree> syntaxTrees,
            string outputFolder,
            string assemblyFile,
            string symbolsFile)
        {
            using (var assemblyStream = new MemoryStream())
            using (var symbolsStream = new MemoryStream())
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

                assemblyStream.Seek(0, SeekOrigin.Begin);
                symbolsStream.Seek(0, SeekOrigin.Begin);

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                var assemblyBytes = assemblyStream.ToArray();
                logger.LogInformation($"Writing assembly file to \"{assemblyFile}\" with {assemblyBytes.Length} bytes");
                File.WriteAllBytes(assemblyFile, assemblyBytes);

                var symbolBytes = symbolsStream.ToArray();
                logger.LogInformation($"Writing symbol file to \"{symbolsFile}\" with {symbolBytes.Length} bytes");
                File.WriteAllBytes(symbolsFile, symbolBytes);
            }
        }
    }
}
