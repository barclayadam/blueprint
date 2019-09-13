using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

using NLog;

#if !NET472
using System.Runtime.Loader;
#endif

namespace Blueprint.Compiler
{
    /// <summary>
    /// Use to compile C# code to in memory assemblies using the Roslyn compiler
    /// </summary>
    public class AssemblyGenerator
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly GenerationRules rules;

        private readonly List<MetadataReference> references = new List<MetadataReference>();
        private readonly List<Assembly> assemblies = new List<Assembly>();

        private readonly List<(string Reference, Exception Exception)> referenceErrors = new List<(string Reference, Exception Exception)>();
        private readonly List<SourceFile> files = new List<SourceFile>();

        public AssemblyGenerator(GenerationRules rules)
        {
            this.rules = rules;

            ReferenceAssemblyContainingType<object>();
            ReferenceAssemblyContainingType<AssemblyGenerator>();
            ReferenceAssemblyContainingType<Task>();

            foreach (var assembly in rules.Assemblies)
            {
                ReferenceAssembly(assembly);
            }
        }

        /// <summary>
        /// Reference the assembly containing the type "T"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ReferenceAssemblyContainingType<T>()
        {
            ReferenceAssembly(typeof(T).GetTypeInfo().Assembly);
        }

        /// <summary>
        /// Tells Roslyn to reference the given assembly and any of its dependencies
        /// when compiling code
        /// </summary>
        /// <param name="assembly"></param>
        public void ReferenceAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                return;
            }

            if (assemblies.Contains(assembly))
            {
                return;
            }

            assemblies.Add(assembly);

            try
            {
                var referencePath = CreateAssemblyReference(assembly);

                if (referencePath == null)
                {
                    return;
                }

                var alreadyReferenced = references.Any(x => x.Display == referencePath);
                if (alreadyReferenced)
                    return;

                var reference = MetadataReference.CreateFromFile(referencePath);

                references.Add(reference);

                foreach (var assemblyName in assembly.GetReferencedAssemblies())
                {
                    var referencedAssembly = Assembly.Load(assemblyName);
                    ReferenceAssembly(referencedAssembly);
                }
            }
            catch (Exception e)
            {
                referenceErrors.Add((assembly.FullName, e));
            }
        }

        /// <summary>
        /// Compile the code passed into this method to a new assembly
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Assembly Generate()
        {
            // TODO: Switch based on environment?
            var compileStrategy = new ToFileCompileStrategy();
            var encoding = Encoding.UTF8;
            var assemblyName = rules.AssemblyName ?? throw new InvalidOperationException("AssemblyName must be set on GenerationRules");

            var syntaxTrees = new List<SyntaxTree>();

            foreach (var f in files)
            {
                var sourceCodePath = f.FileName;

                var buffer = encoding.GetBytes(f.Code);
                var sourceText = SourceText.From(buffer, buffer.Length, encoding, canBeEmbedded: true);

                var syntaxTree = CSharpSyntaxTree.ParseText(
                    sourceText,
                    new CSharpParseOptions(),
                    path: sourceCodePath);

                var syntaxRootNode = syntaxTree.GetRoot() as CSharpSyntaxNode;
                var encodedSyntaxTree = CSharpSyntaxTree.Create(syntaxRootNode, null, sourceCodePath, encoding);

                syntaxTrees.Add(encodedSyntaxTree);
            }

            Log.Debug("Generating compilation unit for {0}. Optimization level is {1}", assemblyName, rules.OptimizationLevel);

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: syntaxTrees,
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOptimizationLevel(rules.OptimizationLevel)
            );

            return compileStrategy.Compile(
                compilation,
                (result) =>
                {
                    if (!result.Success)
                    {
                        var failures = result.Diagnostics.Where(diagnostic =>
                            diagnostic.IsWarningAsError ||
                            diagnostic.Severity == DiagnosticSeverity.Error);

                        var exceptionMessage = new StringBuilder();

                        exceptionMessage.AppendLine("Compilation failed");
                        exceptionMessage.AppendLine();

                        foreach (var failure in failures)
                        {
                            exceptionMessage.AppendLine($"{failure.Id}: {failure.GetMessage()} @ {failure.Location}");
                        }

                        exceptionMessage.AppendLine();

                        if (referenceErrors.Any())
                        {
                            exceptionMessage.AppendLine("Assembly reference errors (these may be the reason compilation fails)");
                            exceptionMessage.AppendLine();

                            foreach (var e in referenceErrors)
                            {
                                exceptionMessage.AppendLine($" {e.Reference}: {e.Exception.Message}");
                            }

                            exceptionMessage.AppendLine();
                        }

                        throw new CompilationException(exceptionMessage.ToString())
                        {
                            Code = string.Join("\n\n", files.Select(f => f.Code))
                        };
                    }
                });
        }

        private static string CreateAssemblyReference(Assembly assembly)
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
            {
                return null;
            }

            return assembly.Location;
        }

        private interface ICompileStrategy
        {
            Assembly Compile(CSharpCompilation compilation, Action<EmitResult> check);
        }

        private class InMemoryOnlyCompileStrategy : ICompileStrategy
        {
            public Assembly Compile(CSharpCompilation compilation, Action<EmitResult> check)
            {
                Log.Info("Compiling source to an in-memory DLL with embedded source");

                using (var assemblyStream = new MemoryStream())
                using (var symbolsStream = new MemoryStream())
                {
                    var emitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.Embedded);
                    var embeddedTexts = compilation.SyntaxTrees.Select(s => EmbeddedText.FromSource(s.FilePath, s.GetText()));

                    var result = compilation.Emit(
                        peStream: assemblyStream,
                        pdbStream: symbolsStream,
                        embeddedTexts: embeddedTexts,
                        options: emitOptions);

                    check(result);

                    assemblyStream.Seek(0, SeekOrigin.Begin);
                    symbolsStream.Seek(0, SeekOrigin.Begin);

#if !NET472
                    return AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
#else
                    return Assembly.Load(assemblyStream.ToArray(), symbolsStream.ToArray());
#endif
                }
            }
        }

        private class ToFileCompileStrategy : ICompileStrategy
        {
            public Assembly Compile(CSharpCompilation compilation, Action<EmitResult> check)
            {
                var sourceTexts =  compilation.SyntaxTrees.Select(s => s.GetText());
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
                        Log.Info("NOT compiling as previous compilation exists at '{0}'. Hash of compilation is '{1}'", assemblyFile, sourceTextHash);

                        needsCompile = false;
                    }
                }

                if (needsCompile)
                {
                    Log.Info("Compiling source to DLL at '{0}'. Hash of compilation is '{1}'", assemblyFile, sourceTextHash);

                    Compile(compilation, check, compilation.SyntaxTrees, outputFolder, assemblyFile, symbolsFile);

                    File.WriteAllText(manifestFile, sourceTextHash);
                }

#if !NET472
                    return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
#else
                    return Assembly.LoadFrom(assemblyFile);
#endif
            }

            static string GetSha256Hash(string input)
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

            private static void Compile(
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
                    Log.Info($"Writing assembly file to \"{assemblyFile}\" with {assemblyBytes.Length} bytes");
                    File.WriteAllBytes(assemblyFile, assemblyBytes);

                    var symbolBytes = symbolsStream.ToArray();
                    Log.Info($"Writing symbol file to \"{symbolsFile}\" with {symbolBytes.Length} bytes");
                    File.WriteAllBytes(symbolsFile, symbolBytes);
                }
            }
        }

        public void AddFile(string fileName, string code)
        {
            this.files.Add(new SourceFile(fileName, code));
        }

        private class SourceFile
        {
            public SourceFile(string fileName, string code)
            {
                FileName = fileName;
                Code = code;
            }

            public string FileName { get; }

            public string Code { get; }
        }
    }
}
