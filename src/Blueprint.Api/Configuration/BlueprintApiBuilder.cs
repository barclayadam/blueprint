using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public class BlueprintApiBuilder
    {
        private readonly BlueprintApiOptions options;
        private readonly BlueprintPipelineBuilder pipelineBuilder;

        public BlueprintApiBuilder(IServiceCollection services, BlueprintApiOptions options = null)
        {
            Services = services;

            this.options = options ?? new BlueprintApiOptions();
            this.pipelineBuilder = new BlueprintPipelineBuilder(this);

            // The default strategy is to build to a DLL to the temp folder
            Compilation(c => c.UseFileCompileStrategy(Path.Combine(Path.GetTempPath(), "Blueprint.Compiler")));
        }

        public IServiceCollection Services { get; }

        internal BlueprintApiOptions Options => options;

        public BlueprintApiBuilder SetApplicationName(string applicationName)
        {
            Guard.NotNullOrEmpty(nameof(applicationName), applicationName);

            options.WithApplicationName(applicationName);

            return this;
        }

        /// <summary>
        /// Sets the "base" URL of this HTTP API, which is used throughout to, for example, generate API links (see <see cref="ApiResource.Links" />)
        /// with absolute paths.
        /// </summary>
        /// <param name="baseUrl">The fully-qualified URL to set.</param>
        /// <returns>This configurer.</returns>
        public BlueprintApiBuilder SetHttpBaseUrl(string baseUrl)
        {
            Guard.NotNullOrEmpty(nameof(baseUrl), baseUrl);

            options.BaseApiUrl = baseUrl;

            return this;
        }

        public BlueprintApiBuilder ScanForOperations(params Assembly[] assemblies)
        {
            Guard.NotNull(nameof(assemblies), assemblies);

            options.ScanForOperations(assemblies);

            return this;
        }

        public BlueprintApiBuilder AddOperation<T>() where T : IApiOperation
        {
            options.AddOperation<T>();

            return this;
        }

        public BlueprintApiBuilder AddOperation(Type operationType)
        {
            options.AddOperation(operationType);

            return this;
        }

        public BlueprintApiBuilder AddOperations(IEnumerable<Type> operationTypes)
        {
            foreach (var operationType in operationTypes)
            {
                options.AddOperation(operationType);
            }

            return this;
        }

        public BlueprintApiBuilder Pipeline(Action<BlueprintPipelineBuilder> configurer)
        {
            Guard.NotNull(nameof(configurer), configurer);

            configurer(pipelineBuilder);

            return this;
        }

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

            pipelineBuilder.Register();

            options.Rules.AssemblyName ??= options.ApplicationName.Replace(" ", string.Empty).Replace("-", string.Empty);

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
            Services.TryAddScoped<IErrorLogger, ErrorLogger>();
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
