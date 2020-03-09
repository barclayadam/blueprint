﻿using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using Blueprint.Api.Errors;
using Blueprint.Compiler;
using Blueprint.Core;
using Blueprint.Core.Apm;
using Blueprint.Core.Caching;
using Blueprint.Core.Errors;
using Blueprint.Core.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Blueprint.Api.Configuration
{
    /// <summary>
    /// A builder that allows fluent configuration, driven by Intellisense, of a <see cref="BlueprintApiOptions" />
    /// instance.
    /// </summary>
    public class BlueprintApiBuilder
    {
        private readonly BlueprintApiOptions options;
        private readonly BlueprintPipelineBuilder pipelineBuilder;
        private readonly BlueprintApiOperationScanner operationScanner;

        /// <summary>
        /// Initialises a new instance of the <see cref="BlueprintApiBuilder" /> class with the given
        /// <see cref="IServiceCollection" /> in to which all DI registrations will be made.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public BlueprintApiBuilder(IServiceCollection services)
        {
            Services = services;

            options = new BlueprintApiOptions();
            pipelineBuilder = new BlueprintPipelineBuilder(this);
            operationScanner = new BlueprintApiOperationScanner();

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
        /// <returns>This <see cref="BlueprintApiBuilder"/> for further configuration.</returns>
        public BlueprintApiBuilder SetApplicationName(string applicationName)
        {
            Guard.NotNullOrEmpty(nameof(applicationName), applicationName);

            options.ApplicationName = applicationName;
            options.GenerationRules.AssemblyName = applicationName.Replace(" ", string.Empty) + ".Pipelines";

            return this;
        }

        /// <summary>
        /// Sets the "base" URL of this HTTP API, which is used throughout to, for example, generate API links (see <see cref="ApiResource.Links" />)
        /// with absolute paths.
        /// </summary>
        /// <param name="baseUrl">The fully-qualified URL to set.</param>
        /// <returns>This <see cref="BlueprintApiBuilder"/> for further configuration.</returns>
        public BlueprintApiBuilder SetHttpBaseUrl(string baseUrl)
        {
            Guard.NotNullOrEmpty(nameof(baseUrl), baseUrl);

            options.BaseApiUrl = baseUrl;

            return this;
        }

        /// <summary>
        /// Registers an <see cref="IBlueprintApiHost" />, such as a background task processor or HTTP.
        /// </summary>
        /// <param name="host">The host to set.</param>
        /// <returns>This <see cref="BlueprintApiBuilder"/> for further configuration.</returns>
        /// <exception cref="InvalidOperationException">If this method has already been called.</exception>
        public BlueprintApiBuilder UseHost(IBlueprintApiHost host)
        {
            if (options.Host != null)
            {
                throw new InvalidOperationException(
                    $"Cannot set host {host.GetType().Name} as host {options.Host.GetType().Name} has already been registered");
            }

            options.Host = host;

            return this;
        }

        /// <summary>
        /// Configures the <see cref="IApiOperation"/>s of the <see cref="ApiDataModel" /> that will be constructed, allowing
        /// for manual registration as well as scanning operations.
        /// </summary>
        /// <param name="configurer">The action that performs the necessary configuration calls.</param>
        /// <returns>This <see cref="BlueprintApiBuilder"/> for further configuration.</returns>
        public BlueprintApiBuilder Operations(Action<BlueprintApiOperationScanner> configurer)
        {
            Guard.NotNull(nameof(configurer), configurer);

            configurer(operationScanner);

            return this;
        }

        /// <summary>
        /// Configures the pipeline of this API instance.
        /// </summary>
        /// <param name="configurer">The action that performs the necessary configuration calls.</param>
        /// <returns>This <see cref="BlueprintApiBuilder"/> for further configuration.</returns>
        public BlueprintApiBuilder Pipeline(Action<BlueprintPipelineBuilder> configurer)
        {
            Guard.NotNull(nameof(configurer), configurer);

            configurer(pipelineBuilder);

            return this;
        }

        /// <summary>
        /// Configures the compilation (using Roslyn) of this API instance.
        /// </summary>
        /// <param name="configurer">The action that performs the necessary configuration calls.</param>
        /// <returns>This <see cref="BlueprintApiBuilder"/> for further configuration.</returns>
        public BlueprintApiBuilder Compilation(Action<BlueprintCompilationBuilder> configurer)
        {
            Guard.NotNull(nameof(configurer), configurer);

            configurer(new BlueprintCompilationBuilder(this));

            return this;
        }

        internal void Build()
        {
            if (string.IsNullOrEmpty(options.ApplicationName))
            {
                throw new InvalidOperationException("An app name MUST be set");
            }

            if (options.Host == null)
            {
                throw new InvalidOperationException("A host MUST be set");
            }

            pipelineBuilder.Register();
            operationScanner.Register(options.Model);

            options.GenerationRules.AssemblyName ??= options.ApplicationName.Replace(" ", string.Empty).Replace("-", string.Empty);

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
            Services.TryAddSingleton<IApiLinkGenerator, ApiLinkGenerator>();

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
            Services.TryAddScoped<IApmTool, NullApmTool>();

            Services.TryAddSingleton(ArrayPool<byte>.Shared);
            Services.TryAddSingleton(ArrayPool<char>.Shared);

            Services.AddApiOperationHandlers(options.Model.Operations.ToList());
        }
    }
}
