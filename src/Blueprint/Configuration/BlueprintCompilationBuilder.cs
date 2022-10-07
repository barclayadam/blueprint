using System.IO;
using System.Reflection;
using Blueprint.Compiler;
using Blueprint.Compiler.Model;
using Blueprint.Utilities;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Blueprint.Configuration;

public class BlueprintCompilationBuilder
{
    private readonly BlueprintApiBuilder _blueprintApiBuilder;

    internal BlueprintCompilationBuilder(BlueprintApiBuilder blueprintApiBuilder, Assembly callingAssembly, string callerFilePath)
    {
        this._blueprintApiBuilder = blueprintApiBuilder;

        // Given the caller path search up until we hit a directory that has a "bin" child folder, which we can
        // take to mean the root of the project (which in most setups will be the case).
        //
        // This does assume that the calling assembly is the one that was used to call AddBlueprintApi, and means
        // it cannot be pushed to a common / shared project
        var directory = Path.GetDirectoryName(callerFilePath);

        while (directory != null && Directory.Exists(Path.Combine(directory, "bin")) == false)
        {
            directory = Directory.GetParent(directory)?.FullName;
        }

        this._blueprintApiBuilder.Services.Configure<BlueprintApiOptions>(o =>
        {
            o.GenerationRules.AssemblyName = $"{callingAssembly.GetName().Name ?? "Blueprint"}.GeneratedPipelines";
            o.PipelineAssembly = callingAssembly;
            o.GeneratedCodeFolder = directory == null ? null : Path.Combine(directory, "Internal", "Generated", "Blueprint");
            o.ThrowOnSourceChange = CiDetector.IsRunningOnCiServer;
        });

        // By default we will apply a "smart" system where we use the auto strategy in development and
        // static in production.
        this._blueprintApiBuilder.Services.AddSingleton<IApiOperationExecutorBuilder>(s =>
        {
            var hostingEnvironment = s.GetRequiredService<IHostEnvironment>();

            // If in development we want to use Auto so that we can always generate up to date code. In a CI
            // environment we wish to also use Auto, but with ThrowOnSourceChange true so that we can detect
            // outdated / missing generated pipelines
            if (hostingEnvironment.IsDevelopment() || CiDetector.IsRunningOnCiServer)
            {
                return ActivatorUtilities.CreateInstance<AutoApiOperationExecutorBuilder>(s);
            }

            return ActivatorUtilities.CreateInstance<StaticApiOperationExecutorBuilder>(s);
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
            this._blueprintApiBuilder.Services.Configure<BlueprintApiOptions>(o =>
            {
                o.GenerationRules.AssemblyName = assemblyName;
            });
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
    /// <param name="throwOnSourceChange">Whether an exception should be thrown at startup when compiling pipeline if the source has changed.</param>
    /// <returns>This builder.</returns>
    public BlueprintCompilationBuilder UseAutoStrategy(Assembly pipelineAssembly, string generatedCodeFolder, bool throwOnSourceChange = false)
    {
        this._blueprintApiBuilder.Services.Configure<BlueprintApiOptions>(o =>
        {
            o.GenerationRules.AssemblyName = $"{pipelineAssembly.GetName().Name ?? "Blueprint"}.GeneratedPipelines";
            o.PipelineAssembly = pipelineAssembly;
            o.GeneratedCodeFolder = generatedCodeFolder;
            o.ThrowOnSourceChange = throwOnSourceChange;
        });

        this._blueprintApiBuilder.Services.Replace(ServiceDescriptor.Singleton<IApiOperationExecutorBuilder, AutoApiOperationExecutorBuilder>());

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
        this._blueprintApiBuilder.Services.Configure<BlueprintApiOptions>(o =>
        {
            o.PipelineAssembly = pipelineAssembly;
        });

        this._blueprintApiBuilder.Services.Replace(ServiceDescriptor.Singleton<IApiOperationExecutorBuilder, StaticApiOperationExecutorBuilder>());

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

        this._blueprintApiBuilder.Services.Configure<BlueprintApiOptions>(o =>
        {
            o.GenerationRules.VariableSources.Add(variableSource);
        });

        return this;
    }
}
