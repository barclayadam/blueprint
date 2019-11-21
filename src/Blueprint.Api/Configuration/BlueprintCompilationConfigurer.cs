using System;
using Blueprint.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Configuration
{
    public class BlueprintCompilationConfigurer
    {
        private readonly BlueprintApiConfigurer blueprintApiConfigurer;

        internal BlueprintCompilationConfigurer(BlueprintApiConfigurer blueprintApiConfigurer)
        {
            this.blueprintApiConfigurer = blueprintApiConfigurer;
        }

        public BlueprintCompilationConfigurer UseCompileStrategy<T>() where T : class, ICompileStrategy
        {
            blueprintApiConfigurer.Services.AddSingleton<ICompileStrategy, T>();

            return this;
        }

        public BlueprintCompilationConfigurer UseInMemoryCompileStrategy()
        {
            return UseCompileStrategy<InMemoryOnlyCompileStrategy>();
        }

        public BlueprintCompilationConfigurer UseFileCompileStrategy()
        {
            return UseCompileStrategy<ToFileCompileStrategy>();
        }

        public BlueprintCompilationConfigurer UseOptimizationLevel(OptimizationLevel optimizationLevel)
        {
            blueprintApiConfigurer.Options.Rules.OptimizationLevel = optimizationLevel;

            return this;
        }

        public BlueprintCompilationConfigurer ConfigureRules(Action<GenerationRules> editor)
        {
            editor(blueprintApiConfigurer.Options.Rules);

            return this;
        }
    }
}
