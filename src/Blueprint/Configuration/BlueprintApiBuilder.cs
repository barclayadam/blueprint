using System;
using System.Buffers;
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
    public class BlueprintApiBuilder
    {
        private readonly BlueprintApiOptions _options;
        private readonly PipelineBuilder _pipelineBuilder;
        private readonly OperationScanner _operationScanner;
        private readonly ExecutorScanner _executionScanner;

        /// <summary>
        /// Initialises a new instance of the <see cref="BlueprintApiBuilder" /> class with the given
        /// <see cref="IServiceCollection" /> in to which all DI registrations will be made.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public BlueprintApiBuilder(IServiceCollection services)
        {
            this.Services = services;

            this._options = new BlueprintApiOptions();
            this._pipelineBuilder = new PipelineBuilder(this);
            this._operationScanner = new OperationScanner();
            this._executionScanner = new ExecutorScanner();

            if (BlueprintEnvironment.IsPrecompiling)
            {
                // The default strategy is to build to a DLL to the temp folder
                this.Compilation(c => c.UseFileCompileStrategy(Path.GetDirectoryName(typeof(BlueprintApiBuilder).Assembly.Location)));
            }
            else
            {
                // The default strategy is to build to a DLL to the temp folder
                this.Compilation(c => c.UseFileCompileStrategy(Path.Combine(Path.GetTempPath(), "Blueprint.Compiler")));
            }
        }

        /// <summary>
        /// Gets the <see cref="IServiceCollection" /> that dependencies should be registered with.
        /// </summary>
        public IServiceCollection Services { get; }

        internal BlueprintApiOptions Options => this._options;

        /// <summary>
        /// Sets the name of this application, which can be used for naming of the output DLL to avoid
        /// any clashes if multiple APIs exist within a single domain.
        /// </summary>
        /// <param name="applicationName">The name of the application.</param>
        /// <returns>This builder.</returns>
        public BlueprintApiBuilder SetApplicationName(string applicationName)
        {
            Guard.NotNullOrEmpty(nameof(applicationName), applicationName);

            this._options.ApplicationName = applicationName;

            return this;
        }

        /// <summary>
        /// Configures the scanner that will search for operations and handlers that make the <see cref="ApiDataModel" />
        /// of this Blueprint instance.
        /// </summary>
        /// <param name="scannerAction">The action that performs the necessary configuration calls.</param>
        /// <returns>This builder.</returns>
        public BlueprintApiBuilder Operations(Action<OperationScanner> scannerAction)
        {
            Guard.NotNull(nameof(scannerAction), scannerAction);

            scannerAction(this._operationScanner);

            return this;
        }

        /// <summary>
        /// Adds a single operation to this builder, a shorthand for using the method <see cref="OperationScanner.AddOperation{T}" /> through
        /// the <see cref="Operations"/> method.
        /// </summary>
        /// <param name="configure">An optional action that can modify the created <see cref="ApiOperationDescriptor" />.</param>
        /// <typeparam name="T">The operation type to register.</typeparam>
        /// <returns>This builder.</returns>
        public BlueprintApiBuilder WithOperation<T>(Action<ApiOperationDescriptor> configure = null)
        {
            return this.Operations(o => o.AddOperation<T>($"WithOperation<{typeof(T).Name}>()", configure));
        }

        /// <summary>
        /// Adds a configured handler, plus the associated operation type.
        /// </summary>
        /// <param name="handler">The configured handler to register (as a singleton).</param>
        /// <param name="configure">An optional action that can modify the created <see cref="ApiOperationDescriptor" />.</param>
        /// <typeparam name="T">The operation type.</typeparam>
        /// <returns>This builder.</returns>
        public BlueprintApiBuilder WithHandler<T>(IApiOperationHandler<T> handler, Action<ApiOperationDescriptor> configure = null)
        {
            this.Operations(o => o.AddOperation(typeof(T), $"WithHandler({handler.GetType().Name})", configure));
            this.Services.AddSingleton(handler);

            return this;
        }

        /// <summary>
        /// Configures the pipeline of this API instance.
        /// </summary>
        /// <param name="pipelineAction">The action that performs the necessary configuration calls.</param>
        /// <returns>This builder.</returns>
        public BlueprintApiBuilder Pipeline(Action<PipelineBuilder> pipelineAction)
        {
            Guard.NotNull(nameof(pipelineAction), pipelineAction);

            pipelineAction(this._pipelineBuilder);

            return this;
        }

        /// <summary>
        /// Configures the compilation (using Roslyn) of this API instance.
        /// </summary>
        /// <param name="compilationAction">The action that performs the necessary configuration calls.</param>
        /// <returns>This builder.</returns>
        public BlueprintApiBuilder Compilation(Action<BlueprintCompilationBuilder> compilationAction)
        {
            Guard.NotNull(nameof(compilationAction), compilationAction);

            compilationAction(new BlueprintCompilationBuilder(this));

            return this;
        }

        /// <summary>
        /// Configures how Blueprint scans for executors of registered operations.
        /// </summary>
        /// <param name="executorScanner">The action that performs the necessary configuration calls.</param>
        /// <returns>This builder.</returns>
        public BlueprintApiBuilder Executors(Action<ExecutorScanner> executorScanner)
        {
            Guard.NotNull(nameof(executorScanner), executorScanner);

            executorScanner(this._executionScanner);

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
        public BlueprintApiBuilder AddMessageSource<T>() where T : class, IMessagePopulationSource
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
        public BlueprintApiBuilder AddMessageSource(IMessagePopulationSource source)
        {
            this.Services.AddSingleton(source);

            return this;
        }

        public void Build()
        {
            if (string.IsNullOrEmpty(this._options.ApplicationName))
            {
                throw new InvalidOperationException("An app name MUST be set");
            }

            this._pipelineBuilder.Register();
            this._operationScanner.FindOperations(this._options.Model);

            this._options.GenerationRules.AssemblyName ??= this._options.ApplicationName.Replace(" ", string.Empty) + ".Pipelines";

            this.Services.AddLogging();

            // Register the collection that built the service provider so that the code generation can inspect the registrations and
            // generate better code (i.e. inject singleton services in to the pipeline executor instead of getting them at operation execution time)
            this.Services.AddSingleton(this.Services);

            // Compilation
            this.Services.TryAddSingleton<IAssemblyGenerator, AssemblyGenerator>();
            this.Services.AddSingleton<IApiOperationExecutor>(s => new ApiOperationExecutorBuilder(s.GetRequiredService<ILogger<ApiOperationExecutorBuilder>>()).Build(this._options, s));

            // Model / Links / Options
            this.Services.AddSingleton(this._options);
            this.Services.AddSingleton(this._options.Model);

            // Logging
            this.Services.TryAddSingleton<IErrorLogger, ErrorLogger>();
            this.Services.TryAddSingleton<IExceptionFilter, BasicExceptionFilter>();

            // Cache
            this.Services.TryAddSingleton<ICache, Cache>();
            this.Services.TryAddSingleton(MemoryCache.Default);

            // IoC
            this.Services.TryAddTransient<InstanceFrameProvider, MicrosoftDependencyInjectionInstanceFrameProvider>();

            // Random infrastructure
            this.Services.TryAddScoped<IVersionInfoProvider, NulloVersionInfoProvider>();
            this.Services.TryAddSingleton<IApmTool, NullApmTool>();

            this.Services.TryAddSingleton(ArrayPool<byte>.Shared);
            this.Services.TryAddSingleton(ArrayPool<char>.Shared);

            this._executionScanner.FindAndRegister(
                this._operationScanner,
                this.Services,
                this._options.Model.Operations.ToList());
        }
    }
}
