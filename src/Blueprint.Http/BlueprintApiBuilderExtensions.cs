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
    /// Extensions to <see cref="BlueprintApiBuilder{THost}" /> to register HTTP-specific features.
    /// </summary>
    public static class BlueprintApiBuilderExtensions
    {
        /// <summary>
        /// Registers HTTP-specific functionality and handling to this API instance.
        /// </summary>
        /// <param name="hostBuilder">The host builder to register with.</param>
        /// <returns>This builder.</returns>
        public static BlueprintApiBuilder<HttpHost> Http(this BlueprintApiHostBuilder hostBuilder)
        {
            var apiBuilder = hostBuilder.UseHost<HttpHost>();

            apiBuilder.Services.AddSingleton<IHttpRequestStreamReaderFactory, MemoryPoolHttpRequestStreamReaderFactory>();
            apiBuilder.Services.AddSingleton<IHttpResponseStreamWriterFactory, MemoryPoolHttpResponseStreamWriterFactory>();

            apiBuilder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            apiBuilder.Services.TryAddSingleton<JsonOperationResultOutputFormatter>();
            apiBuilder.Services.TryAddSingleton<IOperationResultOutputFormatter, JsonOperationResultOutputFormatter>();

            apiBuilder.Services.AddScoped<IApiLinkGenerator, ApiLinkGenerator>();

            apiBuilder.AddMessageSource<HttpRouteMessagePopulationSource>();
            apiBuilder.AddMessageSource<HttpBodyMessagePopulationSource>();

            // "Owned" HTTP part sources
            apiBuilder.AddMessageSource(
                HttpPartMessagePopulationSource.Owned<FromCookieAttribute>(c => c.GetProperty("Request").GetProperty(nameof(HttpRequest.Cookies))));

            apiBuilder.AddMessageSource(
                HttpPartMessagePopulationSource.Owned<FromHeaderAttribute>(c => c.GetProperty("Request").GetProperty(nameof(HttpRequest.Headers))));

            apiBuilder.AddMessageSource(
                HttpPartMessagePopulationSource.Owned<FromQueryAttribute>(c => c.GetProperty("Request").GetProperty(nameof(HttpRequest.Query))));

            // Catch-all query string population source
            apiBuilder.AddMessageSource(
                HttpPartMessagePopulationSource.CatchAll(
                    "fromQuery",
                    c => c.GetProperty("Request").GetProperty(nameof(HttpRequest.Query)),
                    c => c.Descriptor.GetFeatureData<HttpOperationFeatureData>().HttpMethod == "GET"));

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

        public static BlueprintApiBuilder<HttpHost> AddHateoasLinks(this BlueprintApiBuilder<HttpHost> apiBuilder)
        {
            // Resource events needs authoriser services to be registered
            BuiltinBlueprintMiddlewares.TryAddAuthorisationServices(apiBuilder.Services);

            apiBuilder.Services.TryAddScoped<IResourceLinkGenerator, EntityOperationResourceLinkGenerator>();

            apiBuilder.Pipeline(p => p.AddMiddleware<LinkGeneratorMiddlewareBuilder>(MiddlewareStage.Execution));

            return apiBuilder;
        }

        public static BlueprintApiBuilder<HttpHost> AddResourceEvents<T>(this BlueprintApiBuilder<HttpHost> pipelineBuilder) where T : class, IResourceEventRepository
        {
            pipelineBuilder.Services.AddScoped<IResourceEventRepository, T>();

            pipelineBuilder.Pipeline(p => p.AddMiddleware<ResourceEventHandlerMiddlewareBuilder>(MiddlewareStage.Execution));

            return pipelineBuilder;
        }
    }
}
