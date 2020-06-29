using System;
using Blueprint.Configuration;
using Blueprint.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using NSwag;

// This should be discoverable when configuring without extra namespace imports
// ReSharper disable once CheckNamespace
namespace Blueprint.Api.Configuration
{
    /// <summary>
    /// Extensions to <see cref="BlueprintApiBuilder" /> to add Application Insights integration.
    /// </summary>
    public static class BlueprintPipelineBuilderExtensions
    {
        /// <summary>
        /// Adds an OpenAPI query that will return an OpenAPI specification at /openapi.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder to configure.</param>
        /// <returns>This <see cref="BlueprintApiBuilder" /> for further configuration.</returns>
        public static BlueprintApiBuilder AddOpenApi(this BlueprintApiBuilder pipelineBuilder)
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
        public static BlueprintApiBuilder AddOpenApi(this BlueprintApiBuilder pipelineBuilder, Action<OpenApiOptions> configure)
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
