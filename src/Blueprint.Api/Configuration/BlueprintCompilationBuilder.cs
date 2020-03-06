using System;
using Blueprint.Compiler;
using Blueprint.Compiler.Model;
using Blueprint.Core;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blueprint.Api.Configuration
{
    public class BlueprintCompilationBuilder
    {
        private readonly BlueprintApiBuilder blueprintApiBuilder;

        internal BlueprintCompilationBuilder(BlueprintApiBuilder blueprintApiBuilder)
        {
            this.blueprintApiBuilder = blueprintApiBuilder;
        }

        /// <summary>
        /// Registers the <see cref="InMemoryOnlyCompileStrategy" /> as compilation strategy
        /// to use.
        /// </summary>
        /// <returns>This builder.</returns>
        public BlueprintCompilationBuilder UseInMemoryCompileStrategy()
        {
            blueprintApiBuilder.Services.AddSingleton<ICompileStrategy, InMemoryOnlyCompileStrategy>();

            return this;
        }

        /// <summary>
        /// Registers the <see cref="UseFileCompileStrategy" /> as compilation strategy, compiling to the
        /// given output folder.
        /// to use.
        /// </summary>
        /// <param name="path">The output folder to store the compiled DLL in.</param>
        /// <returns>This builder.</returns>
        public BlueprintCompilationBuilder UseFileCompileStrategy(string path)
        {
            blueprintApiBuilder.Services.AddSingleton<ICompileStrategy>(
                c => new ToFileCompileStrategy(c.GetRequiredService<ILogger<ToFileCompileStrategy>>(), path));

            return this;
        }

        /// <summary>
        /// Uses the specified optimization level when compiling the pipelines.
        /// </summary>
        /// <param name="optimizationLevel">Optimization level to use.</param>
        /// <returns>This builder.</returns>
        public BlueprintCompilationBuilder UseOptimizationLevel(OptimizationLevel optimizationLevel)
        {
            blueprintApiBuilder.Options.GenerationRules.OptimizationLevel = optimizationLevel;

            return this;
        }

        /// <summary>
        /// Sets the name of the assembly that is generated when compiling the executor pipelines.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to use.</param>
        /// <returns>This builder</returns>
        public BlueprintCompilationBuilder AssemblyName(string assemblyName)
        {
            Guard.NotNullOrEmpty(nameof(assemblyName), assemblyName);

            blueprintApiBuilder.Options.GenerationRules.AssemblyName = assemblyName;

            return this;
        }

        /// <summary>
        /// Customise the <see cref="GenerationRules" /> further, rules that are used when
        /// compiling the pipelines.
        /// </summary>
        /// <param name="editor">The action to run with the <see cref="GenerationRules"/> to modify.</param>
        /// <returns>This builder</returns>
        public BlueprintCompilationBuilder ConfigureRules(Action<GenerationRules> editor)
        {
            editor(blueprintApiBuilder.Options.GenerationRules);

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

            blueprintApiBuilder.Options.GenerationRules.VariableSources.Add(variableSource);

            return this;
        }

        private BlueprintCompilationBuilder UseCompileStrategy<T>() where T : class, ICompileStrategy
        {
            blueprintApiBuilder.Services.AddSingleton<ICompileStrategy, T>();

            return this;
        }
    }
}
