using System;
using Blueprint;
using Blueprint.Configuration;
using Blueprint.Http;
using Blueprint.Http.Formatters;
using Blueprint.Http.Infrastructure;
using Blueprint.Http.MessagePopulation;
using Blueprint.Http.Middleware;
using Blueprint.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;

// Match the DI container namespace so that Blueprint is immediately discoverable
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions to <see cref="BlueprintApiBuilder" /> to register HTTP-specific features.
    /// </summary>
    public static class BlueprintApiBuilderExtensions
    {
        /// <summary>
        /// Registers HTTP-specific functionality and handling to this API instance.
        /// </summary>
        /// <param name="apiBuilder">The builder to register with.</param>
        /// <param name="configureOptions">An optional action that can configure <see cref="BlueprintHttpOptions" />, executed
        /// <c>after</c> the default configuration has been run.</param>
        /// <returns>This builder.</returns>
        public static BlueprintApiBuilder Http(this BlueprintApiBuilder apiBuilder, Action<BlueprintHttpOptions> configureOptions = null)
        {
            apiBuilder.Services.AddSingleton<IHttpRequestStreamReaderFactory, MemoryPoolHttpRequestStreamReaderFactory>();
            apiBuilder.Services.AddSingleton<IHttpResponseStreamWriterFactory, MemoryPoolHttpResponseStreamWriterFactory>();

            apiBuilder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            apiBuilder.Services.AddOptions<BlueprintHttpOptions>()
                .Configure(o =>
                {
                    o.OutputFormatters.Add(new SystemTextJsonResultOutputFormatter(SystemTextJsonResultOutputFormatter.CreateOptions()));

                    configureOptions?.Invoke(o);
                });

            apiBuilder.Services.AddSingleton<IOutputFormatterSelector, DefaultOutputFormatterSelector>();

            apiBuilder.Services.AddScoped<IApiLinkGenerator, ApiLinkGenerator>();

            apiBuilder.AddMessageSource<HttpRouteMessagePopulationSource>();
            apiBuilder.AddMessageSource<HttpBodyMessagePopulationSource>();

            // "Owned" HTTP part sources
            apiBuilder.AddMessageSource(
                HttpPartMessagePopulationSource.Owned<FromCookieAttribute>(
                    c => c.GetProperty("Request").GetProperty(nameof(HttpRequest.Cookies)),
                    false));

            apiBuilder.AddMessageSource(
                HttpPartMessagePopulationSource.Owned<FromHeaderAttribute>(
                    c => c.GetProperty("Request").GetProperty(nameof(HttpRequest.Headers)),
                    true));

            apiBuilder.AddMessageSource(
                HttpPartMessagePopulationSource.Owned<FromQueryAttribute>(
                    c => c.GetProperty("Request").GetProperty(nameof(HttpRequest.Query)),
                    true));

            // Catch-all query string population source
            apiBuilder.AddMessageSource(
                HttpPartMessagePopulationSource.CatchAll(
                    "fromQuery",
                    c => c.GetProperty("Request").GetProperty(nameof(HttpRequest.Query)),
                    c => c.Descriptor.GetFeatureData<HttpOperationFeatureData>().HttpMethod == "GET",
                    true));

            apiBuilder.Services.AddSingleton<IOperationResultExecutor<ValidationFailedOperationResult>, ValidationFailedOperationResultExecutor>();
            apiBuilder.Services.AddSingleton<IOperationResultExecutor<UnhandledExceptionOperationResult>, UnhandledExceptionOperationResultExecutor>();
            apiBuilder.Services.AddSingleton<IOperationResultExecutor<OkResult>, OkResultOperationExecutor>();
            apiBuilder.Services.AddSingleton<OkResultOperationExecutor>();

            apiBuilder.Services.AddSingleton<IContextMetadataProvider, HttpContextMetadataProvider>();

            apiBuilder.Operations(o => o
                .AddOperation<RootMetadataOperation>("AddHttp")
                .AddConvention(new HttpOperationScannerConvention()));

            apiBuilder.Compilation(c => c.AddVariableSource(new HttpVariableSource()));

            return apiBuilder;
        }

        public static BlueprintApiBuilder AddHateoasLinks(this BlueprintApiBuilder apiBuilder)
        {
            // Resource events needs authoriser services to be registered
            BuiltinBlueprintMiddlewares.TryAddAuthorisationServices(apiBuilder.Services);

            apiBuilder.Services.TryAddScoped<IResourceLinkGenerator, EntityOperationResourceLinkGenerator>();

            apiBuilder.Pipeline(p => p.AddMiddleware<LinkGeneratorMiddlewareBuilder>(MiddlewareStage.Execution));

            return apiBuilder;
        }

        public static BlueprintApiBuilder AddResourceEvents<T>(this BlueprintApiBuilder pipelineBuilder) where T : class, IResourceEventRepository
        {
            pipelineBuilder.Services.AddScoped<IResourceEventRepository, T>();

            pipelineBuilder.Pipeline(p => p.AddMiddleware<ResourceEventHandlerMiddlewareBuilder>(MiddlewareStage.Execution));

            return pipelineBuilder;
        }
    }
}
