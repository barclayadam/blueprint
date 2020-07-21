using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using Blueprint.Apm;
using Blueprint.Caching;
using Blueprint.Compiler;
using Blueprint.Errors;
using Blueprint.Middleware;
using Blueprint.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Blueprint.Configuration
{
    /// <summary>
    /// A builder that allows fluent configuration, driven by Intellisense, of a <see cref="BlueprintApiOptions" />
    /// instance.
    /// </summary>
    /// <typeparam name="THost">The type of host.</typeparam>
    public class BlueprintApiBuilder<THost> : IHostBuilder
    {
        private readonly BlueprintApiOptions options;
        private readonly PipelineBuilder<THost> pipelineBuilder;
        private readonly OperationScanner operationScanner;
        private readonly ExecutorScanner executionScanner;

        /// <summary>
        /// Initialises a new instance of the <see cref="BlueprintApiBuilder{THost}" /> class with the given
        /// <see cref="IServiceCollection" /> in to which all DI registrations will be made.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public BlueprintApiBuilder(IServiceCollection services)
        {
            Services = services;

            options = new BlueprintApiOptions();
            pipelineBuilder = new PipelineBuilder<THost>(this);
            operationScanner = new OperationScanner();
            executionScanner = new ExecutorScanner();

            // The default strategy is to build to a DLL to the temp folder
            Compilation(c => c.UseFileCompileStrategy(Path.Combine(Path.GetTempPath(), "Blueprint.Compiler")));
        }

        /// <summary>
        /// Gets the <see cref="IServiceCollection" /> that dependencies should be registered with.
        /// </summary>
        public IServiceCollection Services { get; }

        internal BlueprintApiOptions Options => options;

        /// <summary>
        /// Sets the name of this application, which can be used for naming of the output DLL to avoid
        /// any clashes if multiple APIs exist within a single domain.
        /// </summary>
        /// <param name="applicationName">The name of the application.</param>
        /// <returns>This builder.</returns>
        public BlueprintApiBuilder<THost> SetApplicationName(string applicationName)
        {
            Guard.NotNullOrEmpty(nameof(applicationName), applicationName);

            options.ApplicationName = applicationName;

            return this;
        }

        /// <summary>
        /// Configures the scanner that will search for operations and handlers that make the <see cref="ApiDataModel" />
        /// of this Blueprint instance.
        /// </summary>
        /// <param name="scannerAction">The action that performs the necessary configuration calls.</param>
        /// <returns>This builder.</returns>
        public BlueprintApiBuilder<THost> Operations(Action<OperationScanner> scannerAction)
        {
            Guard.NotNull(nameof(scannerAction), scannerAction);

            scannerAction(operationScanner);

            return this;
        }

        /// <summary>
        /// Adds a single operation to this builder, a shorthand for using the method <see cref="OperationScanner.AddOperation{T}" /> through
        /// the <see cref="Operations"/> method.
        /// </summary>
        /// <param name="source">The source of this handler / operation, to optionally help identify where it came from for diagnostics.</param>
        /// <typeparam name="T">The operation type to register.</typeparam>
        /// <returns>This builder.</returns>
        public BlueprintApiBuilder<THost> WithOperation<T>(string source = null)
        {
            return this.Operations(o => o.AddOperation<T>(source ?? $"WithOperation<{typeof(T).Name}>()"));
        }

        /// <summary>
        /// Adds a configured handler, plus the associated operation type.
        /// </summary>
        /// <param name="handler">The configured handler to register (as a singleton).</param>
        /// <param name="source">The source of this handler / operation, to optionally help identify where it came from for diagnostics.</param>
        /// <typeparam name="T">The operation type.</typeparam>
        /// <returns>This builder.</returns>
        public BlueprintApiBuilder<THost> WithHandler<T>(IApiOperationHandler<T> handler, string source = null)
        {
            this.Operations(o => o.AddOperation(typeof(T), source ?? $"WithHandler({handler.GetType().Name})"));
            this.Services.AddSingleton(handler);

            return this;
        }

        /// <summary>
        /// Configures the pipeline of this API instance.
        /// </summary>
        /// <param name="pipelineAction">The action that performs the necessary configuration calls.</param>
        /// <returns>This builder.</returns>
        public BlueprintApiBuilder<THost> Pipeline(Action<PipelineBuilder<THost>> pipelineAction)
        {
            Guard.NotNull(nameof(pipelineAction), pipelineAction);

            pipelineAction(pipelineBuilder);

            return this;
        }

        /// <summary>
        /// Configures the compilation (using Roslyn) of this API instance.
        /// </summary>
        /// <param name="compilationAction">The action that performs the necessary configuration calls.</param>
        /// <returns>This builder.</returns>
        public BlueprintApiBuilder<THost> Compilation(Action<BlueprintCompilationBuilder<THost>> compilationAction)
        {
            Guard.NotNull(nameof(compilationAction), compilationAction);

            compilationAction(new BlueprintCompilationBuilder<THost>(this));

            return this;
        }

        /// <summary>
        /// Configures how Blueprint scans for executors of registered operations.
        /// </summary>
        /// <param name="executorScanner">The action that performs the necessary configuration calls.</param>
        /// <returns>This builder.</returns>
        public BlueprintApiBuilder<THost> Executors(Action<ExecutorScanner> executorScanner)
        {
            Guard.NotNull(nameof(executorScanner), executorScanner);

            executorScanner(this.executionScanner);

            return this;
        }

        /// <summary>
        /// Adds an <see cref="IMessagePopulationSource" /> that can be used to populate the data
        /// for properties of an operation and mark some as "owned".
        /// </summary>
        /// <typeparam name="T">The type of the source to add.</typeparam>
        /// <returns>This builder.</returns>
        /// <seealso cref="IMessagePopulationSource" />
        /// <seealso cref="MessagePopulationMiddlewareBuilder" />
        public BlueprintApiBuilder<THost> AddMessageSource<T>() where T : class, IMessagePopulationSource
        {
            this.Services.AddSingleton<IMessagePopulationSource, T>();

            return this;
        }

        /// <summary>
        /// Adds an <see cref="IMessagePopulationSource" /> that can be used to populate the data
        /// for properties of an operation and mark some as "owned".
        /// </summary>
        /// <param name="source">The source to add.</param>
        /// <returns>This builder.</returns>
        /// <seealso cref="IMessagePopulationSource" />
        /// <seealso cref="MessagePopulationMiddlewareBuilder" />
        public BlueprintApiBuilder<THost> AddMessageSource(IMessagePopulationSource source)
        {
            this.Services.AddSingleton(source);

            return this;
        }

        void IHostBuilder.Build()
        {
            if (string.IsNullOrEmpty(options.ApplicationName))
            {
                throw new InvalidOperationException("An app name MUST be set");
            }

            pipelineBuilder.Register();
            operationScanner.FindOperations(options.Model);

            options.GenerationRules.AssemblyName ??= options.ApplicationName.Replace(" ", string.Empty) + ".Pipelines";

            Services.AddLogging();

            // Register the collection that built the service provider so that the code generation can inspect the registrations and
            // generate better code (i.e. inject singleton services in to the pipeline executor instead of getting them at operation execution time)
            Services.AddSingleton(Services);

            // Compilation
            Services.TryAddSingleton<IAssemblyGenerator, AssemblyGenerator>();
            Services.AddSingleton<IApiOperationExecutor>(s => new ApiOperationExecutorBuilder(s.GetRequiredService<ILogger<ApiOperationExecutorBuilder>>()).Build(options, s));

            // Model / Links / Options
            Services.AddSingleton(options);
            Services.AddSingleton(options.Model);

            // Logging
            Services.TryAddSingleton<IErrorLogger, ErrorLogger>();
            Services.TryAddSingleton<IExceptionFilter, BasicExceptionFilter>();

            // Cache
            Services.TryAddSingleton<ICache, Cache>();
            Services.TryAddSingleton(MemoryCache.Default);

            // IoC
            Services.TryAddTransient<InstanceFrameProvider, MicrosoftDependencyInjectionInstanceFrameProvider>();

            // Random infrastructure
            Services.TryAddScoped<IVersionInfoProvider, NulloVersionInfoProvider>();
            Services.TryAddSingleton<IApmTool, NullApmTool>();

            Services.TryAddSingleton(ArrayPool<byte>.Shared);
            Services.TryAddSingleton(ArrayPool<char>.Shared);

            executionScanner.FindAndRegister(
                this.operationScanner,
                Services,
                options.Model.Operations.ToList());
        }
    }
}
