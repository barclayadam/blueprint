using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using Blueprint.Api.Errors;
using Blueprint.Api.Infrastructure;
using Blueprint.Compiler;
using Blueprint.Core;
using Blueprint.Core.Apm;
using Blueprint.Core.Caching;
using Blueprint.Core.Errors;
using Blueprint.Core.Tracing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Blueprint.Api.Configuration
{
    public class BlueprintApiConfigurer
    {
        private readonly BlueprintApiOptions options;

        public BlueprintApiConfigurer(IServiceCollection services, BlueprintApiOptions options = null)
        {
            Services = services;

            this.options = options ?? new BlueprintApiOptions();
        }

        public IServiceCollection Services { get; }

        internal BlueprintApiOptions Options => options;

        public BlueprintApiConfigurer SetApplicationName(string applicationName)
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
        public BlueprintApiConfigurer SetHttpBaseUrl(string baseUrl)
        {
            Guard.NotNullOrEmpty(nameof(baseUrl), baseUrl);

            options.BaseApiUrl = baseUrl;

            return this;
        }

        public BlueprintApiConfigurer ScanForOperations(params Assembly[] assemblies)
        {
            Guard.NotNull(nameof(assemblies), assemblies);

            options.ScanForOperations(assemblies);

            return this;
        }

        public BlueprintApiConfigurer AddOperation<T>() where T : IApiOperation
        {
            options.AddOperation<T>();

            return this;
        }

        public BlueprintApiConfigurer AddOperation(Type operationType)
        {
            options.AddOperation(operationType);

            return this;
        }

        public BlueprintApiConfigurer AddOperations(IEnumerable<Type> operationTypes)
        {
            foreach (var operationType in operationTypes)
            {
                options.AddOperation(operationType);
            }

            return this;
        }

        public BlueprintApiConfigurer Pipeline(Action<BlueprintMiddlewareConfigurer> configurer)
        {
            Guard.NotNull(nameof(configurer), configurer);

            var blueprintMiddlewareConfigurer = new BlueprintMiddlewareConfigurer(this);

            configurer(blueprintMiddlewareConfigurer);

            blueprintMiddlewareConfigurer.Register();

            return this;
        }

        public BlueprintApiConfigurer Compilation(Action<BlueprintCompilationConfigurer> configurer)
        {
            Guard.NotNull(nameof(configurer), configurer);

            configurer(new BlueprintCompilationConfigurer(this));

            return this;
        }

        internal void Build()
        {
            if (string.IsNullOrEmpty(options.ApplicationName))
            {
                throw new InvalidOperationException("An app name MUST be set");
            }

            options.Rules.AssemblyName ??= options.ApplicationName.Replace(" ", string.Empty).Replace("-", string.Empty);

            Services.AddLogging();

            // Register the collection that built the service provider so that the code generation can inspect the registrations and
            // generate better code (i.e. inject singleton services in to the pipeline executor instead of getting them at operation execution time)
            Services.AddSingleton(Services);

            // Compilation
            Services.TryAddSingleton<IAssemblyGenerator, AssemblyGenerator>();
            Services.TryAddSingleton<ICompileStrategy, ToFileCompileStrategy>();
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
            Services.TryAddTransient<IInstanceFrameProvider, DefaultInstanceFrameProvider>();

            // Random infrastructure
            Services.TryAddScoped<IVersionInfoProvider, NulloVersionInfoProvider>();
            Services.TryAddScoped<IApmTool, NullApmTool>();

            Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            Services.AddSingleton<IHttpRequestStreamReaderFactory, MemoryPoolHttpRequestStreamReaderFactory>();
            Services.AddSingleton<IHttpResponseStreamWriterFactory, MemoryPoolHttpResponseStreamWriterFactory>();

            Services.TryAddSingleton(ArrayPool<byte>.Shared);
            Services.TryAddSingleton(ArrayPool<char>.Shared);

            Services.AddApiOperationHandlers(options.Model.Operations.ToList());
        }
    }
}
