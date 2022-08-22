using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Blueprint.Compiler;
using Blueprint.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint;

/// <summary>
/// A <see cref="IApiOperationExecutorBuilder" /> that is recommended for development environments that
/// will write the generated code to disk, in addition to load the pre-existing types. Only missing or changed
/// pipelines will be compiled to an in-memory assembly as required.
/// </summary>
/// <remarks>
/// The recommended workflow is using this builder in development so that the generated code is written to disk and
/// in production loaded directly in to the assembly and loaded using a <see cref="StaticApiOperationExecutorBuilder" />
/// to completely avoid the cost of building the pipeline's and compiling the generated code.
/// </remarks>
public class AutoApiOperationExecutorBuilder : IApiOperationExecutorBuilder
{
    private readonly Assembly _pipelineAssembly;
    private readonly string _folder;

    private readonly IAssemblyGenerator _inner;

    /// <summary>
    /// Initialises a new instance of the <see cref="AutoApiOperationExecutorBuilder"/> class.
    /// </summary>
    /// <param name="pipelineAssembly">The assembly from which to load previously generated pipeline types.</param>
    /// <param name="folder">The folder to write the generated code to.</param>
    public AutoApiOperationExecutorBuilder(Assembly pipelineAssembly, string folder)
    {
        this._pipelineAssembly = pipelineAssembly;
        this._folder = folder;
        this._inner = new AssemblyGenerator(new InMemoryOnlyCompileStrategy());
    }

    /// <inheritdoc/>
    public IApiOperationExecutor Build(BlueprintApiOptions options, IServiceProvider serviceProvider)
    {
        var model = options.Model;

        // Upfront check to ensure the folder exists before trying to write any code files
        Directory.CreateDirectory(this._folder);

        var preloadedTypes = new List<Type>();
        var typeToCreationMappings = new Dictionary<ApiOperationDescriptor, Func<Type>>();
        var sourceCodeMappings = new Dictionary<ApiOperationDescriptor, Func<string>>();

        foreach (var operation in options.Model.Operations)
        {
            var operationType = operation.TryFindPipelineHandler(this._pipelineAssembly);

            if (operationType == null)
            {
                continue;
            }

            preloadedTypes.Add(operationType);
        }

        var assembly = ApiOperationExecutorBuilderHelper.StartAssembly(options);

        using var serviceScope = serviceProvider.CreateScope();

        foreach (var operation in model.Operations)
        {
            var pipelineExecutorType = ApiOperationExecutorBuilderHelper.BuildPipeline(model, options, operation, assembly, serviceScope.ServiceProvider);

            typeToCreationMappings.Add(
                operation,
                () => pipelineExecutorType.CompiledType);

            sourceCodeMappings.Add(
                operation,
                () => pipelineExecutorType.GeneratedSourceCode);
        }

        assembly.CompileAll(new IncrementalAssemblyGenerator(preloadedTypes, this._folder, this._inner));

        return new CodeGennedExecutor(serviceProvider, model, typeToCreationMappings, sourceCodeMappings);
    }

    private class IncrementalAssemblyGenerator : IAssemblyGenerator
    {
        private readonly IEnumerable<Type> _preexistingTypes;
        private readonly string _folder;
        private readonly IAssemblyGenerator _inner;

        public IncrementalAssemblyGenerator(IEnumerable<Type> preexistingTypes, string folder, IAssemblyGenerator inner)
        {
            this._preexistingTypes = preexistingTypes;
            this._folder = folder;
            this._inner = inner;
        }

        public void ReferenceAssembly(Assembly assembly)
        {
            this._inner.ReferenceAssembly(assembly);
        }

        public void AddFile(GeneratedType generatedType, string code)
        {
            var fileName = $"{generatedType.TypeName}.cs";
            var sourceFilePath = Path.Combine(this._folder, fileName);

            if (File.Exists(sourceFilePath))
            {
                var existingCode = File.ReadAllText(sourceFilePath);

                if (existingCode.Equals(code))
                {
                    if (this._preexistingTypes.Any(t => t.Namespace == generatedType.Namespace && t.Name == generatedType.TypeName) == false)
                    {
                        throw new InvalidOperationException("The generated code is the same as the existing code, but the type could not be found in the loaded assembly.\n\n" +
                                                            "This means that the output code folder is NOT contained in the compilation unit for the configured types assembly.");
                    }

                    // This type has already been loaded, so will NOT be added to the
                    // compilation
                    return;
                }
            }

            File.WriteAllText(sourceFilePath, code);

            this._inner.AddFile(generatedType, code);
        }

        public Type[] Generate(GenerationRules rules)
        {
            var newlyCompiled = this._inner.Generate(rules);

            // It is important we return the newly compiled types first, so that the existing types are always
            // overriden if there was a change to the source (FirstOrDefault is used to find the compiled type)
            return newlyCompiled.Concat(this._preexistingTypes).ToArray();
        }
    }
}
