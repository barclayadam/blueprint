using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using Blueprint.Api.Authorisation;
using Blueprint.Api.Errors;
using Blueprint.Api.Formatters;
using Blueprint.Api.Infrastructure;
using Blueprint.Api.Middleware;
using Blueprint.Api.Validation;
using Blueprint.Compiler;
using Blueprint.Core;
using Blueprint.Core.Apm;
using Blueprint.Core.Authorisation;
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
        private readonly Dictionary<MiddlewareStage, List<IMiddlewareBuilder>> middlewareStages = new Dictionary<MiddlewareStage, List<IMiddlewareBuilder>>();

        private readonly BlueprintApiOptions options;

        public BlueprintApiConfigurer(IServiceCollection services, BlueprintApiOptions options = null)
        {
            Services = services;

            this.options = options ?? new BlueprintApiOptions();
        }

        public IServiceCollection Services { get; }

        public BlueprintApiConfigurer SetApplicationName(string applicationName)
        {
            Guard.NotNullOrEmpty(nameof(applicationName), applicationName);

            options.WithApplicationName(applicationName);

            return this;
        }

        public BlueprintApiConfigurer ScanForOperations(params Assembly[] assemblies)
        {
            Guard.NotNull(nameof(assemblies), assemblies);

            options.ScanForOperations(assemblies);

            return this;
        }

        public BlueprintApiConfigurer Middlewares(Action<BlueprintMiddlewareConfigurer> configurer)
        {
            Guard.NotNull(nameof(configurer), configurer);

            configurer(new BlueprintMiddlewareConfigurer(this));

            return this;
        }

        public void AddMiddlewareBuilderToStage<T>(MiddlewareStage middlewareStage)
            where T : IMiddlewareBuilder, new()
        {
            if (middlewareStages.TryGetValue(middlewareStage, out var middlewareTypes))
            {
                middlewareTypes.Add(new T());
            }
            else
            {
                middlewareStages.Add(middlewareStage, new List<IMiddlewareBuilder> { new T() });
            }
        }

        internal void Build()
        {
            if (string.IsNullOrEmpty(options.ApplicationName))
            {
                throw new InvalidOperationException("An app name MUST be set");
            }

            options.Rules.AssemblyName = options.Rules.AssemblyName ?? options.ApplicationName.Replace(" ", string.Empty).Replace("-", string.Empty);

            ComposeMiddlewareBuilders();

            // Register the collection that built the service provider so that the code generation can inspect the registrations and
            // generate better code (i.e. inject singleton services in to the pipeline executor instead of getting them at operation execution time)
            Services.AddSingleton(Services);

            Services.AddSingleton(options);
            Services.AddSingleton(options.Model);
            Services.AddSingleton<ApiConfiguration>();
            Services.AddSingleton<ApiLinkGenerator>();
            Services.AddSingleton<AssemblyGenerator>();
            Services.AddSingleton<ToFileCompileStrategy>();
            Services.AddSingleton<InMemoryOnlyCompileStrategy>();
            Services.AddSingleton<AssemblyGenerator>();
            Services.AddSingleton<IApiOperationExecutor>(s => new ApiOperationExecutorBuilder(s.GetRequiredService<ILogger<ApiOperationExecutorBuilder>>()).Build(options, s));

            // Logging
            Services.TryAddScoped<IErrorLogger, ErrorLogger>();

            // Authentication / Authorisation
            Services.TryAddScoped<IApiAuthoriserAggregator, ApiAuthoriserAggregator>();
            Services.TryAddScoped<IClaimInspector, ClaimInspector>();

            Services.AddScoped<IApiAuthoriser, ClaimsRequiredApiAuthoriser>();
            Services.AddScoped<IApiAuthoriser, MustBeAuthenticatedApiAuthoriser>();

            // Validation
            Services.TryAddSingleton<IValidationSource, DataAnnotationsValidationSource>();
            Services.TryAddSingleton<IValidationSource, BlueprintValidationSource>();
            Services.TryAddSingleton<IValidationSourceBuilder, DataAnnotationsValidationSourceBuilder>();
            Services.TryAddSingleton<IValidationSourceBuilder, BlueprintValidationSourceBuilder>();
            Services.TryAddSingleton<IValidator, BlueprintValidator>();

            // Cache
            Services.TryAddSingleton<ICache, Cache>();
            Services.TryAddSingleton(MemoryCache.Default);
            Services.TryAddSingleton<IExceptionFilter, BasicExceptionFilter>();

            // IoC
            Services.TryAddTransient<IInstanceFrameProvider, DefaultInstanceFrameProvider>();

            // Formatters
            Services.TryAddSingleton<JsonTypeFormatter>();
            Services.TryAddSingleton<ITypeFormatter, JsonTypeFormatter>();

            // Linking
            Services.TryAddScoped<IResourceLinkGenerator, EntityOperationResourceLinkGenerator>();

            // Random infrastructure
            Services.TryAddScoped<IVersionInfoProvider, NulloVersionInfoProvider>();
            Services.TryAddScoped<IApmTool, NullApmTool>();

            Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            Services.TryAddScoped(_ => ArrayPool<char>.Shared);
            Services.TryAddScoped(_ => ArrayPool<byte>.Shared);

            Services.AddSingleton<IHttpRequestStreamReaderFactory, MemoryPoolHttpRequestStreamReaderFactory>();
            Services.AddSingleton<IHttpResponseStreamWriterFactory, MemoryPoolHttpResponseStreamWriterFactory>();

            Services.AddApiOperationHandlers(options.Model.Operations);
        }

        private void ComposeMiddlewareBuilders()
        {
            if (options.Middlewares.Any())
            {
                return;
            }

            AddMiddlewareBuilders(MiddlewareStage.OperationChecks);
            AddMiddlewareBuilders(MiddlewareStage.PreExecute);

            // Execute
            options.UseMiddlewareBuilder<OperationExecutorMiddlewareBuilder>();
            options.UseMiddlewareBuilder<FormatterMiddlewareBuilder>();

            AddMiddlewareBuilders(MiddlewareStage.PostExecute);
        }

        private void AddMiddlewareBuilders(MiddlewareStage middlewareStage)
        {
            if (!middlewareStages.TryGetValue(middlewareStage, out var middlewareTypes))
            {
                return;
            }

            foreach (var middlewareType in middlewareTypes)
            {
                options.Middlewares.Add(middlewareType);
            }
        }
    }
}
