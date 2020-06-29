using Blueprint.Http;
using Blueprint.Http.Formatters;
using Blueprint.Http.Infrastructure;
using Blueprint.Http.MessagePopulation;
using Blueprint.Http.Middleware;
using Blueprint.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace For discoverability we add to existing namespace
namespace Blueprint.Configuration
{
    /// <summary>
    /// Extensions to <see cref="BlueprintApiBuilder" /> to register HTTP-specific features.
    /// </summary>
    public static class BlueprintApiBuilderExtensions
    {
        /// <summary>
        /// Registers HTTP-specific functionality and handling to this API instance.
        /// </summary>
        /// <param name="apiBuilder">The API builder to register with.</param>
        /// <returns>This builder.</returns>
        public static BlueprintApiBuilder AddHttp(this BlueprintApiBuilder apiBuilder)
        {
            apiBuilder.Services.AddSingleton<IHttpRequestStreamReaderFactory, MemoryPoolHttpRequestStreamReaderFactory>();
            apiBuilder.Services.AddSingleton<IHttpResponseStreamWriterFactory, MemoryPoolHttpResponseStreamWriterFactory>();

            apiBuilder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            apiBuilder.Services.TryAddSingleton<JsonOperationResultOutputFormatter>();
            apiBuilder.Services.TryAddSingleton<IOperationResultOutputFormatter, JsonOperationResultOutputFormatter>();

            apiBuilder.Services.AddSingleton<IMessagePopulationSource, HttpRouteMessagePopulationSource>();
            apiBuilder.Services.AddSingleton<IMessagePopulationSource, HttpBodyMessagePopulationSource>();

            apiBuilder.Services.AddScoped<IApiLinkGenerator, ApiLinkGenerator>();

            // "Owned" HTTP part sources
            apiBuilder.Services.AddSingleton<IMessagePopulationSource>(
                HttpPartMessagePopulationSource.Owned<FromCookieAttribute>(c => c.GetProperty("Request").GetProperty(nameof(HttpRequest.Cookies))));

            apiBuilder.Services.AddSingleton<IMessagePopulationSource>(
                HttpPartMessagePopulationSource.Owned<FromHeaderAttribute>(c => c.GetProperty("Request").GetProperty(nameof(HttpRequest.Headers))));

            apiBuilder.Services.AddSingleton<IMessagePopulationSource>(
                HttpPartMessagePopulationSource.Owned<FromQueryAttribute>(c => c.GetProperty("Request").GetProperty(nameof(HttpRequest.Query))));

            // Catch-all query string population source
            apiBuilder.Services.AddSingleton<IMessagePopulationSource>(
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

            apiBuilder.UseHost(new HttpBlueprintApiHost());

            return apiBuilder;
        }

        public static BlueprintPipelineBuilder AddHateoasLinks(this BlueprintPipelineBuilder pipelineBuilder)
        {
            // Resource events needs authoriser services to be registered
            BuiltinBlueprintMiddlewares.TryAddAuthServices(pipelineBuilder);

            pipelineBuilder.Services.TryAddScoped<IResourceLinkGenerator, EntityOperationResourceLinkGenerator>();

            pipelineBuilder.AddMiddleware<LinkGeneratorMiddlewareBuilder>(MiddlewareStage.Execution);

            return pipelineBuilder;
        }

        public static BlueprintPipelineBuilder AddResourceEvents<T>(this BlueprintPipelineBuilder pipelineBuilder) where T : class, IResourceEventRepository
        {
            pipelineBuilder.Services.AddScoped<IResourceEventRepository, T>();

            pipelineBuilder.AddMiddleware<ResourceEventHandlerMiddlewareBuilder>(MiddlewareStage.Execution);

            return pipelineBuilder;
        }
    }
}
