using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using Blueprint.Api.Authorisation;
using Blueprint.Api.Errors;
using Blueprint.Api.Formatters;
using Blueprint.Api.Infrastructure;
using Blueprint.Api.Middleware;
using Blueprint.Compiler;
using Blueprint.Core;
using Blueprint.Core.Caching;
using Blueprint.Core.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blueprint.Api.Configuration
{
    public class BlueprintApiConfigurer
    {
        private readonly Dictionary<MiddlewareStage, List<Type>> middlewareStages = new Dictionary<MiddlewareStage, List<Type>>();

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
            where T : IMiddlewareBuilder
        {
            if (middlewareStages.TryGetValue(middlewareStage, out var middlewareTypes))
            {
                middlewareTypes.Add(typeof(T));
            }
            else
            {
                middlewareStages.Add(middlewareStage, new List<Type> { typeof(T) });
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

            Services.AddSingleton(options);
            Services.AddSingleton(options.Model);
            Services.AddSingleton<ApiConfiguration>();
            Services.AddSingleton<ApiLinkGenerator>();
            Services.AddSingleton<AssemblyGenerator>();
            Services.AddSingleton<ToFileCompileStrategy>();
            Services.AddSingleton<InMemoryOnlyCompileStrategy>();
            Services.AddSingleton<AssemblyGenerator>();
            Services.AddSingleton<IApiOperationExecutor>(s => new ApiOperationExecutorBuilder(s.GetRequiredService<ILogger<ApiOperationExecutorBuilder>>()).Build(options, s));

            // Logging only?
            Services.AddScoped<IErrorLogger, ErrorLogger>();

            // Auth only?
            Services.AddScoped<IApiAuthoriser, MustBeAuthenticatedApiAuthoriser>();
            Services.AddScoped<IApiAuthoriserAggregator, ApiAuthoriserAggregator>();

            Services.AddSingleton<ITypeFormatter, JsonTypeFormatter>();
            Services.AddSingleton<JsonTypeFormatter>();

            Services.AddScoped<IResourceLinkGenerator, EntityOperationResourceLinkGenerator>();
            Services.AddScoped<IApiAuthoriser, MustBeAuthenticatedApiAuthoriser>();
            Services.AddScoped<IResourceLinkGenerator, EntityOperationResourceLinkGenerator>();

            Services.AddSingleton<ICache, Cache>();
            Services.AddSingleton(MemoryCache.Default);
            Services.AddSingleton<IExceptionFilter, BasicExceptionFilter>();

            Services.AddTransient<IInstanceFrameProvider, DefaultInstanceFrameProvider>();

            // Random infrastructure
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
