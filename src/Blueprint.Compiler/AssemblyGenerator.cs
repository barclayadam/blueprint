using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;

namespace Blueprint.Compiler
{
    /// <summary>
    /// Use to compile C# code to in memory assemblies using the Roslyn compiler.
    /// </summary>
    public class AssemblyGenerator : IAssemblyGenerator
    {
        private readonly ILogger<AssemblyGenerator> logger;
        private readonly ICompileStrategy compileStrategy;

        private readonly List<Assembly> referencedAssemblies = new List<Assembly>();

        private readonly List<(string Reference, Exception Exception)> referenceErrors = new List<(string Reference, Exception Exception)>();
        private readonly List<SourceFile> files = new List<SourceFile>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyGenerator" /> class.
        /// </summary>
        /// <param name="logger">A logger for this class.</param>
        /// <param name="compileStrategy">The compilation strategy to use to actually build and load the assemblies.</param>
        public AssemblyGenerator(ILogger<AssemblyGenerator> logger, ICompileStrategy compileStrategy)
        {
            this.logger = logger;
            this.compileStrategy = compileStrategy;

            ReferenceAssemblyContainingType<object>();
            ReferenceAssemblyContainingType<AssemblyGenerator>();
            ReferenceAssemblyContainingType<Task>();
        }

        /// <inheritdoc />
        public void ReferenceAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                return;
            }

            if (referencedAssemblies.Contains(assembly))
            {
                return;
            }

            referencedAssemblies.Add(assembly);
        }

        /// <inheritdoc />
        public void AddFile(string fileName, string code)
        {
            files.Add(new SourceFile(fileName, code));
        }

        /// <summary>
        /// Compile the code passed into this method to a new assembly which is loaded in to the current application.
        /// </summary>
        /// <param name="rules">Rules that are used to control the generation of the <see cref="Assembly"/>.</param>
        /// <returns>A newly constructed (and loaded) Assembly based on registered source files and given generation rules.</returns>
        public Assembly Generate(GenerationRules rules)
        {
            if (string.IsNullOrEmpty(rules.AssemblyName))
            {
                throw new InvalidOperationException("AssemblyName must be set on GenerationRules");
            }

            var encoding = Encoding.UTF8;
            var sourceTextHash = CreateSourceHash(encoding);
            var assemblyName = rules.AssemblyName + ".dll";

            var existingAssembly = compileStrategy.TryLoadExisting(sourceTextHash, assemblyName);

            if (existingAssembly != null)
            {
                return existingAssembly;
            }

            var syntaxTrees = new List<SyntaxTree>();
            var parseOptions = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None);

            foreach (var f in files)
            {
                var sourceCodePath = f.FileName;

                var syntaxTree = CSharpSyntaxTree.ParseText(
                    SourceText.From(f.Code, encoding),
                    parseOptions,
                    path: sourceCodePath);

                syntaxTrees.Add(syntaxTree);
            }

            logger.LogDebug("Generating compilation unit for {0}. Optimization level is {1}", assemblyName, rules.OptimizationLevel);

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: syntaxTrees,
                references: GetAllReferences(),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithDeterministic(true)
                    .WithConcurrentBuild(false)
                    .WithOptimizationLevel(rules.OptimizationLevel));

            return compileStrategy.Compile(
                sourceTextHash,
                compilation,
                (result) =>
                {
                    if (!result.Success)
                    {
                        var failures = result.Diagnostics
                            .Where(diagnostic =>
                                diagnostic.IsWarningAsError ||
                                diagnostic.Severity == DiagnosticSeverity.Error)
                            .ToList();

                        var exceptionMessage = new StringBuilder();

                        exceptionMessage.AppendLine("Compilation failed");
                        exceptionMessage.AppendLine();

                        foreach (var failure in failures)
                        {
                            exceptionMessage.AppendLine(failure.ToString());

                            if (failure.Location.IsInSource)
                            {
                                var span = failure.Location.GetLineSpan();
                                var mappedSpan = failure.Location.GetMappedLineSpan();

                                if (!mappedSpan.IsValid || !span.IsValid)
                                {
                                    continue;
                                }

                                var sourceFile = failure.Location.SourceTree.ToString();
                                var fileLines = sourceFile.Split('\n');
                                var erroredLine = mappedSpan.Span.Start.Line;

                                // We try to output:
                                //     [ line before]
                                //     line with error
                                //        ^^^ indicators of issue
                                //     [line after]
                                TryOutputLine(fileLines, erroredLine - 1, exceptionMessage);
                                TryOutputLine(fileLines, erroredLine, exceptionMessage);

                                exceptionMessage.AppendLine(new string(' ', span.StartLinePosition.Character) +
                                                            new string('^', failure.Location.SourceSpan.Length));

                                TryOutputLine(fileLines, erroredLine + 1, exceptionMessage);
                            }
                        }

                        exceptionMessage.AppendLine();

                        if (referenceErrors.Any())
                        {
                            exceptionMessage.AppendLine("Assembly reference errors (these may be the reason compilation fails)");
                            exceptionMessage.AppendLine();

                            foreach (var (reference, exception) in referenceErrors)
                            {
                                exceptionMessage.AppendLine($" {reference}: {exception.Message}");
                            }

                            exceptionMessage.AppendLine();
                        }

                        var allCode = string.Join("\n\n", files.Select(f => f.Code));

                        throw new CompilationException(exceptionMessage.ToString()) {Failures = failures, Code = allCode,};
                    }
                });
        }

        private List<MetadataReference> GetAllReferences()
        {
            var references = new List<MetadataReference>();

            void Traverse(Assembly assembly)
            {
                try
                {
                    var referencePath = AssemblyLocationOrNull(assembly);

                    if (referencePath == null)
                    {
                        return;
                    }

                    var alreadyReferenced = references.Any(x => x.Display == referencePath);
                    if (alreadyReferenced)
                    {
                        return;
                    }

                    var reference = MetadataReference.CreateFromFile(referencePath);

                    references.Add(reference);

                    foreach (var assemblyName in assembly.GetReferencedAssemblies())
                    {
                        var referencedAssembly = Assembly.Load(assemblyName);

                        Traverse(referencedAssembly);
                    }
                }
                catch (Exception e)
                {
                    referenceErrors.Add((assembly.FullName, e));
                }
            }

            foreach (var assembly in referencedAssemblies)
            {
                Traverse(assembly);
            }

            return references;
        }

        private string CreateSourceHash(Encoding encoding)
        {
            HashAlgorithm hasher = MD5.Create();
            hasher.Initialize();

            foreach (var f in files)
            {
                var fileBytes = encoding.GetBytes(f.Code);

                hasher.TransformBlock(fileBytes, 0, fileBytes.Length, null, 0);
            }

            hasher.TransformFinalBlock(new byte[0], 0, 0);
            var hash = hasher.Hash;

            return BitConverter.ToString(hash);
        }

        private void ReferenceAssemblyContainingType<T>()
        {
            ReferenceAssembly(typeof(T).GetTypeInfo().Assembly);
        }

        private static void TryOutputLine(IReadOnlyList<string> fileLines, int line, StringBuilder exceptionMessage)
        {
            if (line <= 0 || line >= fileLines.Count)
            {
                return;
            }

            var lineToOutput = fileLines[line];
            exceptionMessage.AppendLine(lineToOutput);
        }

        private static string AssemblyLocationOrNull(Assembly assembly)
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
            {
                return null;
            }

            return assembly.Location;
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

            public override string ToString()
            {
                return FileName;
            }
        }
    }
}
