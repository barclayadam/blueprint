using System;
using Blueprint.Compiler;
using Blueprint.Core;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blueprint.Api.Configuration
{
    public class BlueprintCompilationBuilder
    {
        private readonly BlueprintApiConfigurer blueprintApiConfigurer;

        internal BlueprintCompilationBuilder(BlueprintApiConfigurer blueprintApiConfigurer)
        {
            this.blueprintApiConfigurer = blueprintApiConfigurer;
        }

        /// <summary>
        /// Registers the <see cref="InMemoryOnlyCompileStrategy" /> as compilation strategy
        /// to use.
        /// </summary>
        /// <returns>This builder.</returns>
        public BlueprintCompilationBuilder UseInMemoryCompileStrategy()
        {
            blueprintApiConfigurer.Services.AddSingleton<ICompileStrategy, InMemoryOnlyCompileStrategy>();

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
            blueprintApiConfigurer.Services.AddSingleton<ICompileStrategy>(
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
            blueprintApiConfigurer.Options.Rules.OptimizationLevel = optimizationLevel;

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

            blueprintApiConfigurer.Options.Rules.AssemblyName = assemblyName;

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
            editor(blueprintApiConfigurer.Options.Rules);

            return this;
        }

        private BlueprintCompilationBuilder UseCompileStrategy<T>() where T : class, ICompileStrategy
        {
            blueprintApiConfigurer.Services.AddSingleton<ICompileStrategy, T>();

            return this;
        }
    }
}
