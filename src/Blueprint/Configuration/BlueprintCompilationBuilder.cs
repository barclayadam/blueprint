using System.Reflection;
using Blueprint.Compiler;
using Blueprint.Compiler.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Blueprint.Configuration;

public class BlueprintCompilationBuilder
{
    private readonly BlueprintApiBuilder _blueprintApiBuilder;

    internal BlueprintCompilationBuilder(BlueprintApiBuilder blueprintApiBuilder)
    {
        this._blueprintApiBuilder = blueprintApiBuilder;

        // By default we will apply a "smart" system where we use the auto strategy in development and
        // static in production.
        this._blueprintApiBuilder.Services.AddSingleton<IApiOperationExecutorBuilder>(s =>
        {
            var hostingEnvironment = s.GetRequiredService<IHostEnvironment>();

            var isDevelopment = hostingEnvironment.IsDevelopment();

            if (isDevelopment)
            {
                return new AutoApiOperationExecutorBuilder(blueprintApiBuilder.Options.PipelineAssembly, blueprintApiBuilder.Options.GeneratedCodeFolder);
            }

            return new StaticApiOperationExecutorBuilder(blueprintApiBuilder.Options.PipelineAssembly);
        });
    }

    /// <summary>
    /// Registers the <see cref="InMemoryOnlyCompileStrategy" /> as compilation strategy
    /// to use.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to use for the generated code. If this is <c>null</c>
    /// then the assembly name will be auto picked.</param>
    /// <returns>This builder.</returns>
    public BlueprintCompilationBuilder UseInMemoryStrategy([CanBeNull] string assemblyName = null)
    {
        this._blueprintApiBuilder.Services.Replace(ServiceDescriptor.Singleton<IApiOperationExecutorBuilder, InMemoryApiOperationExecutorBuilder>());

        if (assemblyName != null)
        {
            this._blueprintApiBuilder.Options.GenerationRules.AssemblyName = assemblyName;
        }

        return this;
    }

    /// <summary>
    /// Uses a <see cref="AutoApiOperationExecutorBuilder" /> that means the pipelines could have been been built
    /// previously and compiled in to the given assembly, but if not will be compiled in-memory and source written
    /// to the given folder for subsequent compilation into the assembly.
    /// </summary>
    /// <param name="pipelineAssembly">The assembly to load pre-existing pipeline handlers from.</param>
    /// <param name="generatedCodeFolder">The folder into which we should write the pipeline handler source files.</param>
    /// <returns>This builder.</returns>
    public BlueprintCompilationBuilder UseAutoStrategy(Assembly pipelineAssembly, string generatedCodeFolder)
    {
        this._blueprintApiBuilder.Services.Replace(ServiceDescriptor.Singleton<IApiOperationExecutorBuilder>(
            c => new AutoApiOperationExecutorBuilder(pipelineAssembly, generatedCodeFolder)));

        return this;
    }

    /// <summary>
    /// Uses a <see cref="StaticApiOperationExecutorBuilder" /> that means the pipelines must have all been built
    /// previously and compiled in to the given assembly.
    /// to use.
    /// </summary>
    /// <param name="pipelineAssembly">The assembly to load pre-existing pipeline handlers from.</param>
    /// <returns>This builder.</returns>
    public BlueprintCompilationBuilder UseStaticStrategy(Assembly pipelineAssembly)
    {
        this._blueprintApiBuilder.Services.Replace(ServiceDescriptor.Singleton<IApiOperationExecutorBuilder>(
            c => new StaticApiOperationExecutorBuilder(pipelineAssembly)));

        return this;
    }

    /// <summary>
    /// Adds a new <see cref="IVariableSource" /> for use in compilation of the pipelines.
    /// </summary>
    /// <param name="variableSource">The variable source to add.</param>
    /// <returns>This compilation builder.</returns>
    public BlueprintCompilationBuilder AddVariableSource(IVariableSource variableSource)
    {
        Guard.NotNull(nameof(variableSource), variableSource);

        this._blueprintApiBuilder.Options.GenerationRules.VariableSources.Add(variableSource);

        return this;
    }
}