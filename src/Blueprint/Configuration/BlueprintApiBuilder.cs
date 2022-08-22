using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using Blueprint.Caching;
using Blueprint.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        private readonly BlueprintCompilationBuilder _blueprintCompilationBuilder;

        /// <summary>
        /// Initialises a new instance of the <see cref="BlueprintApiBuilder" /> class with the given
        /// <see cref="IServiceCollection" /> in to which all DI registrations will be made.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="callingAssembly">The assembly that was the "calling assembly" of <see cref="ServiceCollectionExtensions.AddBlueprintApi" />, used as
        ///     a default for <see cref="BlueprintApiOptions.PipelineAssembly" />.</param>
        /// <param name="callerFilePath">The caller's file path of the AddBlueprint method, used to determine project path to determine where to
        /// place generated files.</param>
        internal BlueprintApiBuilder(IServiceCollection services, Assembly callingAssembly, string callerFilePath)
        {
            this.Services = services;

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

            this._options = new BlueprintApiOptions
            {
                PipelineAssembly = callingAssembly,
                GeneratedCodeFolder = directory == null ? null : Path.Combine(directory, "Internal", "Generated", "Blueprint"),
            };

            this._pipelineBuilder = new PipelineBuilder(this);
            this._operationScanner = new OperationScanner();
            this._executionScanner = new ExecutorScanner();
            this._blueprintCompilationBuilder = new BlueprintCompilationBuilder(this);

            // Register core middleware that is safe to have in every pipeline.
            this.AddValidation();
        }

        /// <summary>
        /// Gets the <see cref="IServiceCollection" /> that dependencies should be registered with.
        /// </summary>
        public IServiceCollection Services { get; }

        internal BlueprintApiOptions Options => this._options;

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

            compilationAction(this._blueprintCompilationBuilder);

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

        internal void Build()
        {
            this._pipelineBuilder.Register();
            this._operationScanner.FindOperations(this._options.Model);

            this.Services.AddLogging();

            // Register the collection that built the service provider so that the code generation can inspect the registrations and
            // generate better code (i.e. inject singleton services in to the pipeline executor instead of getting them at operation execution time)
            this.Services.AddSingleton(this.Services);

            // Compilation
            this.Services.AddSingleton(s => s.GetRequiredService<IApiOperationExecutorBuilder>().Build(this._options, s));

            // Model / Links / Options
            this.Services.AddSingleton(this._options);
            this.Services.AddSingleton(this._options.Model);

            // Cache
            this.Services.TryAddSingleton<ICache, Cache>();
            this.Services.TryAddSingleton(MemoryCache.Default);

            // IoC
            this.Services.TryAddTransient<InstanceFrameProvider, MicrosoftDependencyInjectionInstanceFrameProvider>();

            // Random infrastructure
            this.Services.TryAddSingleton(ArrayPool<byte>.Shared);
            this.Services.TryAddSingleton(ArrayPool<char>.Shared);

            this._executionScanner.FindAndRegister(
                this._operationScanner,
                this.Services,
                this._options.Model.Operations.ToList());
        }
    }
}
