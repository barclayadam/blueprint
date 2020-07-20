using System;
using Blueprint.Configuration;
using Blueprint.Http;
using Blueprint.OpenApi;
using NSwag;

// Match the DI container namespace so that Blueprint is immediately discoverable
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions to <see cref="BlueprintApiBuilder" /> to add Application Insights integration.
    /// </summary>
    public static class BlueprintApiBuilderExtensions
    {
        /// <summary>
        /// Adds an OpenAPI query that will return an OpenAPI specification at /openapi.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder to configure.</param>
        /// <returns>This <see cref="BlueprintApiBuilder" /> for further configuration.</returns>
        public static BlueprintApiBuilder<HttpHost> AddOpenApi(this BlueprintApiBuilder<HttpHost> pipelineBuilder)
        {
            pipelineBuilder.Services.AddOptions<OpenApiOptions>().Configure(o =>
            {
                o.PostConfigure = d =>
                {
                    d.Info = new OpenApiInfo
                    {
                        Title = "Auto-generated API specification",
                    };
                };

                o.AddSchemaProcessor<BlueprintLinkSchemaProcessor>();
            });

            pipelineBuilder.Operations(o => o.AddOperation<OpenApiQuery>("AddOpenApi"));

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds an OpenAPI query that will return an OpenAPI specification at /openapi.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder to configure.</param>
        /// <param name="configure">An action to configure the <see cref="OpenApiOptions"/>.</param>
        /// <returns>This <see cref="BlueprintApiBuilder" /> for further configuration.</returns>
        public static BlueprintApiBuilder<HttpHost> AddOpenApi(this BlueprintApiBuilder<HttpHost> pipelineBuilder, Action<OpenApiOptions> configure)
        {
            pipelineBuilder.Services.AddOptions<OpenApiOptions>().Configure(o =>
            {
                o.AddSchemaProcessor<BlueprintLinkSchemaProcessor>();

                configure(o);
            });

            pipelineBuilder.Operations(o => o.AddOperation<OpenApiQuery>("AddOpenApi"));

            return pipelineBuilder;
        }
    }
}
