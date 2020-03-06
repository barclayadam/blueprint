using Blueprint.Api.Authorisation;
using Blueprint.Api.Http;
using Blueprint.Api.Http.Formatters;
using Blueprint.Api.Http.Infrastructure;
using Blueprint.Api.Http.MessagePopulation;
using Blueprint.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace Same namespace to ensure Intellisense automatically picks up
namespace Blueprint.Api.Configuration
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

            apiBuilder.Services.TryAddSingleton<IClaimsIdentityProvider, HttpRequestClaimsIdentityProvider>();

            apiBuilder.Services.AddScoped<IMessagePopulationSource, HttpRouteMessagePopulationSource>();
            apiBuilder.Services.AddScoped<IMessagePopulationSource, HttpBodyMessagePopulationSource>();
            apiBuilder.Services.AddScoped<IMessagePopulationSource, HttpQueryStringMessagePopulationSource>();

            apiBuilder.Services.AddSingleton<IOperationResultExecutor<UnhandledExceptionOperationResult>, UnhandledExceptionOperationResultExecutor>();
            apiBuilder.Services.AddSingleton<IOperationResultExecutor<OkResult>, OkResultOperationExecutor>();
            apiBuilder.Services.AddSingleton<OkResultOperationExecutor>();

            apiBuilder.Services.AddSingleton<IContextMetadataProvider, HttpContextMetadataProvider>();

            apiBuilder.Compilation(c => c.AddVariableSource(new HttpVariableSource()));

            return apiBuilder;
        }
    }
}
