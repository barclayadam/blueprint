using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Blueprint.Compiler
{
    public interface IAssemblyGenerator
    {
        /// <summary>
        /// Tells Roslyn to reference the given assembly and any of its dependencies
        /// when compiling code.
        /// </summary>
        /// <param name="assembly">The assembly to reference.</param>
        void ReferenceAssembly(Assembly assembly);

        /// <summary>
        /// Adds a file to this assembly with the specified name and code.
        /// </summary>
        /// <param name="fileName">The name of the file, which may contain</param>
        /// <param name="code">The code of the file.</param>
        /// <exception cref="ArgumentException">If a file of the same name already exists.</exception>
        void AddFile(GeneratedType fileName, string code);

        /// <summary>
        /// Compile the code passed into this method to a new assembly which is loaded in to the current application.
        /// </summary>
        /// <param name="rules">Rules that are used to control the generation of the <see cref="Assembly"/>.</param>
        /// <returns>A newly constructed (and loaded) Assembly based on registered source files and given generation rules.</returns>
        Type[] Generate(GenerationRules rules);
    }

    /// <summary>
    /// Used to compile C# code to in memory assemblies using the Roslyn compiler.
    /// </summary>
    public class AssemblyGenerator : IAssemblyGenerator
    {
        private readonly ICompileStrategy _compileStrategy;

        private readonly List<Assembly> _referencedAssemblies = new();

        private readonly List<(string Reference, Exception Exception)> _referenceErrors = new();
        private readonly List<SourceFile> _files = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyGenerator" /> class.
        /// </summary>
        /// <param name="compileStrategy">The compilation strategy to use to actually build and load the assemblies.</param>
        public AssemblyGenerator(ICompileStrategy compileStrategy)
        {
            this._compileStrategy = compileStrategy;

            this.ReferenceAssemblyContainingType<object>();
            this.ReferenceAssemblyContainingType<AssemblyGenerator>();
            this.ReferenceAssemblyContainingType<Task>();
        }

        /// <summary>
        /// Tells Roslyn to reference the given assembly and any of its dependencies
        /// when compiling code.
        /// </summary>
        /// <param name="assembly">The assembly to reference.</param>
        public void ReferenceAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                return;
            }

            if (this._referencedAssemblies.Contains(assembly))
            {
                return;
            }

            this._referencedAssemblies.Add(assembly);
        }

        /// <summary>
        /// Adds a file to this assembly with the specified name and code.
        /// </summary>
        /// <param name="generatedType">The type this file represents.</param>
        /// <param name="code">The code of the file.</param>
        /// <exception cref="ArgumentException">If a file of the same name already exists.</exception>
        public void AddFile(GeneratedType generatedType, string code)
        {
            var fileName = $"{generatedType.Namespace.Replace(".", "/")}/{generatedType.TypeName}.cs";
            
            if (this._files.Any(f => f.FileName == fileName))
            {
                throw new ArgumentException($"A source file with the name {fileName} has already been added", nameof(fileName));
            }

            this._files.Add(new SourceFile(fileName, code));
        }

        /// <summary>
        /// Compile the code passed into this method to a new assembly which is loaded in to the current application.
        /// </summary>
        /// <param name="rules">Rules that are used to control the generation of the <see cref="Assembly"/>.</param>
        /// <returns>A newly constructed (and loaded) Assembly based on registered source files and given generation rules.</returns>
        public Type[] Generate(GenerationRules rules)
        {
            if (string.IsNullOrEmpty(rules.AssemblyName))
            {
                throw new InvalidOperationException("AssemblyName must be set on GenerationRules");
            }

            var encoding = Encoding.UTF8;
            var assemblyName = rules.AssemblyName + ".dll";

            var syntaxTrees = new List<SyntaxTree>();
            var parseOptions = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None);

            foreach (var f in this._files)
            {
                var sourceCodePath = f.FileName;

                var syntaxTree = CSharpSyntaxTree.ParseText(
                    SourceText.From(f.Code, encoding),
                    parseOptions,
                    path: sourceCodePath);

                syntaxTrees.Add(syntaxTree);
            }

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: syntaxTrees,
                references: this.GetAllReferences(),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithDeterministic(true)
                    .WithConcurrentBuild(false)
                    .WithOptimizationLevel(rules.OptimizationLevel));

            var assembly = this._compileStrategy.Compile(compilation,
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

                        if (this._referenceErrors.Any())
                        {
                            exceptionMessage.AppendLine("Assembly reference errors (these may be the reason compilation fails)");
                            exceptionMessage.AppendLine();

                            foreach (var (reference, exception) in this._referenceErrors)
                            {
                                exceptionMessage.AppendLine($" {reference}: {exception.Message}");
                            }

                            exceptionMessage.AppendLine();
                        }

                        var allCode = string.Join("\n\n", this._files.Select(f => f.Code));

                        throw new CompilationException(exceptionMessage.ToString(), failures, allCode);
                    }
                });

            return assembly.GetExportedTypes().ToArray();
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
                    this._referenceErrors.Add((assembly.FullName, e));
                }
            }

            foreach (var assembly in this._referencedAssemblies)
            {
                Traverse(assembly);
            }

            return references;
        }

        private void ReferenceAssemblyContainingType<T>()
        {
            this.ReferenceAssembly(typeof(T).GetTypeInfo().Assembly);
        }

        private class SourceFile
        {
            public SourceFile(string fileName, string code)
            {
                this.FileName = fileName;
                this.Code = code;
            }

            public string FileName { get; }

            public string Code { get; }

            public override string ToString()
            {
                return this.FileName;
            }
        }
    }
}
