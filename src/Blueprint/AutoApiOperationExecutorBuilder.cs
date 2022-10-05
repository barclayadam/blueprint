using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Blueprint.Compiler;
using Blueprint.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<BlueprintApiOptions> _options;
    private readonly ApiDataModel _dataModel;
    private readonly ILogger<AutoApiOperationExecutorBuilder> _logger;

    private readonly IAssemblyGenerator _assemblyGenerator;

    /// <summary>
    /// Initialises a new instance of the <see cref="AutoApiOperationExecutorBuilder"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="options">The configured <see cref="BlueprintApiOptions" />.</param>
    /// <param name="dataModel">The configured <see cref="ApiDataModel" />.</param>
    /// <param name="logger">A logger to indicate when pipeline types are being compiled.</param>
    public AutoApiOperationExecutorBuilder(
        IServiceProvider serviceProvider,
        IOptions<BlueprintApiOptions> options,
        ApiDataModel dataModel,
        ILogger<AutoApiOperationExecutorBuilder> logger)
    {
        this._serviceProvider = serviceProvider;
        this._options = options;
        this._dataModel = dataModel;
        this._logger = logger;
        this._assemblyGenerator = new AssemblyGenerator(new InMemoryOnlyCompileStrategy());
    }

    /// <inheritdoc/>
    public IApiOperationExecutor Build()
    {
        // Upfront check to ensure the folder exists before trying to write any code files
        Directory.CreateDirectory(this._options.Value.GeneratedCodeFolder);

        var preloadedTypes = new List<Type>();
        var typeToCreationMappings = new Dictionary<ApiOperationDescriptor, Func<Type>>();
        var sourceCodeMappings = new Dictionary<ApiOperationDescriptor, Func<string>>();

        foreach (var operation in this._dataModel.Operations)
        {
            var operationType = operation.TryFindPipelineHandler(this._options.Value.PipelineAssembly);

            if (operationType == null)
            {
                continue;
            }

            preloadedTypes.Add(operationType);
        }

        var assembly = ApiOperationExecutorBuilderHelper.StartAssembly(this._options.Value);

        using var serviceScope = this._serviceProvider.CreateScope();

        foreach (var operation in this._dataModel.Operations)
        {
            var pipelineExecutorType = ApiOperationExecutorBuilderHelper.BuildPipeline(this._dataModel, this._options.Value, operation, assembly, serviceScope.ServiceProvider);

            typeToCreationMappings.Add(
                operation,
                () => pipelineExecutorType.CompiledType);

            sourceCodeMappings.Add(
                operation,
                () => pipelineExecutorType.GeneratedSourceCode);
        }

        assembly.CompileAll(new IncrementalAssemblyGenerator(preloadedTypes, this._options.Value, this._assemblyGenerator, this._logger));

        this._logger.LogInformation("Completed incremental compilation for {OperationCount} operations", typeToCreationMappings.Count);

        return new CodeGennedExecutor(this._serviceProvider, this._dataModel, typeToCreationMappings, sourceCodeMappings);
    }

    private class IncrementalAssemblyGenerator : IAssemblyGenerator
    {
        private readonly IEnumerable<Type> _preexistingTypes;
        private readonly BlueprintApiOptions _options;
        private readonly IAssemblyGenerator _inner;
        private readonly ILogger _logger;

        public IncrementalAssemblyGenerator(IEnumerable<Type> preexistingTypes, BlueprintApiOptions options, IAssemblyGenerator inner, ILogger logger)
        {
            this._preexistingTypes = preexistingTypes;
            this._options = options;
            this._inner = inner;
            this._logger = logger;
        }

        public void ReferenceAssembly(Assembly assembly)
        {
            this._inner.ReferenceAssembly(assembly);
        }

        public void AddFile(GeneratedType generatedType, string code)
        {
            var fileName = $"{generatedType.TypeName}.cs";
            var sourceFilePath = Path.Combine(this._options.GeneratedCodeFolder, fileName);

            if (File.Exists(sourceFilePath))
            {
                var existingCode = File.ReadAllText(sourceFilePath);

                if (existingCode.Equals(code))
                {
                    if (this._preexistingTypes.Any(t => t.Namespace == generatedType.Namespace && t.Name == generatedType.TypeName) == false)
                    {
                        throw new InvalidOperationException($"The generated code for {generatedType.Namespace}.{generatedType.TypeName} is the same as the existing code, but the type could not be found in the loaded assembly.\n\n" +
                                                            $"Ensure the output code folder {this._options.GeneratedCodeFolder} is contained in the configured types assembly {this._options.PipelineAssembly.GetName().Name}.");
                    }

                    this._logger.LogTrace(
                        "Existing pipeline {GeneratedTypeNamespace}.{GeneratedTypeName} is up to date",
                        generatedType.Namespace,
                        generatedType.TypeName);

                    // This type has already been loaded, so will NOT be added to the
                    // compilation
                    return;
                }
            }

            if (this._options.ThrowOnSourceChange)
            {
                throw new InvalidOperationException($"The generated code for {generatedType.Namespace}.{generatedType.TypeName} is outdated and must be recreated.");
            }

            this._logger.LogInformation(
                "Adding compilation unit for {GeneratedTypeNamespace}.{GeneratedTypeName}",
                generatedType.Namespace,
                generatedType.TypeName);

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
