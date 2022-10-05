using System;
using System.Collections.Generic;
using Blueprint.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blueprint;

/// <summary>
/// A <see cref="IApiOperationExecutorBuilder" /> that will only load existing pipeline handlers, throwing
/// an exception if any are missing.
/// </summary>
public class StaticApiOperationExecutorBuilder : IApiOperationExecutorBuilder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<BlueprintApiOptions> _options;
    private readonly ApiDataModel _dataModel;
    private readonly ILogger<StaticApiOperationExecutorBuilder> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="StaticApiOperationExecutorBuilder"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="options">The configured <see cref="BlueprintApiOptions" />.</param>
    /// <param name="dataModel">The configured <see cref="ApiDataModel" />.</param>
    /// <param name="logger">A logger to indicate when pipeline types are being compiled.</param>
    public StaticApiOperationExecutorBuilder(
        IServiceProvider serviceProvider,
        IOptions<BlueprintApiOptions> options,
        ApiDataModel dataModel,
        ILogger<StaticApiOperationExecutorBuilder> logger)
    {
        this._serviceProvider = serviceProvider;
        this._options = options;
        this._dataModel = dataModel;
        this._logger = logger;
    }

    /// <inheritdoc/>
    public IApiOperationExecutor Build()
    {
        var typeToCreationMappings = new Dictionary<ApiOperationDescriptor, Func<Type>>();

        foreach (var operation in this._dataModel.Operations)
        {
            var operationType = operation.TryFindPipelineHandler(this._options.Value.PipelineAssembly);

            if (operationType == null)
            {
                throw new InvalidOperationException(
                    @$"A pipeline could not be found for the operation {operation.OperationType.FullName} when using the {nameof(StaticApiOperationExecutorBuilder)}. This can happen because:

 * You intended to use an Auto or InMemoryOnly strategy but have set Static instead

 * Have made changes to include a new operation type but have not run the application in Auto mode to generate the static pipeline classes");
            }

            typeToCreationMappings.Add(operation, () => operationType);
        }

        this._logger.LogInformation("Successfully loaded {OperationCount} operations", typeToCreationMappings.Count);

        return new CodeGennedExecutor(
            this._serviceProvider,
            this._dataModel,
            typeToCreationMappings,
            null);
    }
}
