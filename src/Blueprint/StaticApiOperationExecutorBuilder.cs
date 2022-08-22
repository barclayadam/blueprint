using System;
using System.Collections.Generic;
using System.Reflection;
using Blueprint.Configuration;

namespace Blueprint;

/// <summary>
/// A <see cref="IApiOperationExecutorBuilder" /> that will only load existing pipeline handlers, throwing
/// an exception if any are missing.
/// </summary>
public class StaticApiOperationExecutorBuilder : IApiOperationExecutorBuilder
{
    private readonly Assembly _pipelineAssembly;

    /// <summary>
    /// Initialises a new instance of the <see cref="StaticApiOperationExecutorBuilder"/> class.
    /// </summary>
    /// <param name="pipelineAssembly">The assembly that contains the pre-built types.</param>
    public StaticApiOperationExecutorBuilder(Assembly pipelineAssembly)
    {
        this._pipelineAssembly = pipelineAssembly;
    }

    /// <inheritdoc/>
    public IApiOperationExecutor Build(BlueprintApiOptions blueprintApiOptions, IServiceProvider serviceProvider)
    {
        var typeToCreationMappings = new Dictionary<ApiOperationDescriptor, Func<Type>>();

        foreach (var operation in blueprintApiOptions.Model.Operations)
        {
            var operationType = operation.TryFindPipelineHandler(this._pipelineAssembly);

            if (operationType == null)
            {
                throw new InvalidOperationException(
                    @$"A pipeline could not be found for the operation {operation.OperationType.FullName} when using the {nameof(StaticApiOperationExecutorBuilder)}. This can happen because:

 * You intended to use an Auto or InMemoryOnly strategy but have set Static instead

 * Have made changes to include a new operation type but have not run the application in Auto mode to generate the static pipeline classes");
            }

            typeToCreationMappings.Add(operation, () => operationType);
        }

        return new CodeGennedExecutor(
            serviceProvider,
            blueprintApiOptions.Model,
            typeToCreationMappings,
            null);
    }
}
