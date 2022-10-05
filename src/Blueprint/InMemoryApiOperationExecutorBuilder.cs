using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Compiler;
using Blueprint.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blueprint;

/// <summary>
/// A <see cref="IApiOperationExecutorBuilder" /> that will only create pipelines in an in-memory assembly, no existing
/// pipeline types will be used and nothing will be stored to disk.
/// </summary>
public class InMemoryApiOperationExecutorBuilder : IApiOperationExecutorBuilder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<BlueprintApiOptions> _options;
    private readonly ApiDataModel _dataModel;
    private readonly ILogger<InMemoryApiOperationExecutorBuilder> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="InMemoryApiOperationExecutorBuilder"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="options">The configured <see cref="BlueprintApiOptions" />.</param>
    /// <param name="dataModel">The configured <see cref="ApiDataModel" />.</param>
    /// <param name="logger">A logger to indicate when pipeline types are being compiled.</param>
    public InMemoryApiOperationExecutorBuilder(
        IServiceProvider serviceProvider,
        IOptions<BlueprintApiOptions> options,
        ApiDataModel dataModel,
        ILogger<InMemoryApiOperationExecutorBuilder> logger)
    {
        this._serviceProvider = serviceProvider;
        this._options = options;
        this._dataModel = dataModel;
        this._logger = logger;
    }

    /// <inheritdoc/>
    public IApiOperationExecutor Build()
    {
        var model = this._dataModel;
        var assembly = ApiOperationExecutorBuilderHelper.StartAssembly(this._options.Value);
        var typeToCreationMappings = new Dictionary<ApiOperationDescriptor, Func<Type>>();
        var sourceCodeMappings = new Dictionary<ApiOperationDescriptor, Func<string>>();

        using var serviceScope = this._serviceProvider.CreateScope();

        foreach (var operation in model.Operations)
        {
            var pipelineExecutorType = ApiOperationExecutorBuilderHelper.BuildPipeline(model, this._options.Value, operation, assembly, serviceScope.ServiceProvider);

            typeToCreationMappings.Add(
                operation,
                () => pipelineExecutorType.CompiledType);

            sourceCodeMappings.Add(
                operation,
                () => pipelineExecutorType.GeneratedSourceCode);
        }

        this._logger.LogInformation("Compiling {OperationCount} operations into in-memory assembly", typeToCreationMappings.Count);

        assembly.CompileAll(new AssemblyGenerator(new InMemoryOnlyCompileStrategy()));

        return new CodeGennedExecutor(this._serviceProvider, model, typeToCreationMappings, sourceCodeMappings);
    }
}
