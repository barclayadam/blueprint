using System;
using System.Collections.Generic;
using Blueprint.Compiler;
using Blueprint.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint;

/// <summary>
/// A <see cref="IApiOperationExecutorBuilder" /> that will only create pipelines in an in-memory assembly, no existing
/// pipeline types will be used and nothing will be stored to disk.
/// </summary>
public class InMemoryApiOperationExecutorBuilder : IApiOperationExecutorBuilder
{
    /// <inheritdoc/>
    public IApiOperationExecutor Build(BlueprintApiOptions options, IServiceProvider serviceProvider)
    {
        var model = options.Model;
        var assembly = ApiOperationExecutorBuilderHelper.StartAssembly(options);
        var typeToCreationMappings = new Dictionary<ApiOperationDescriptor, Func<Type>>();
        var sourceCodeMappings = new Dictionary<ApiOperationDescriptor, Func<string>>();

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

        assembly.CompileAll(new AssemblyGenerator(new InMemoryOnlyCompileStrategy()));

        return new CodeGennedExecutor(serviceProvider, model, typeToCreationMappings, sourceCodeMappings);
    }
}
