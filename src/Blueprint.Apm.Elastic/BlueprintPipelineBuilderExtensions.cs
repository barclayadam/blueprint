using Blueprint.Apm.Elastic;
using Blueprint.Core.Apm;
using Elastic.Apm.Api;
using Microsoft.Extensions.DependencyInjection;

// This should be discoverable when configuring without extra namespace imports
// ReSharper disable once CheckNamespace
namespace Blueprint.Api.Configuration
{
    /// <summary>
    /// Extensions to <see cref="BlueprintApiBuilder" /> to add Elastic APM integration.
    /// </summary>
    public static class BlueprintPipelineBuilderExtensions
    {
        /// <summary>
        /// Adds Elastic APM integration to this API, registering an <see cref="IApmTool" /> to allow
        /// tracking dependencies, and middleware that will create or configure an <see cref="ITransaction" />.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder to configure.</param>
        /// <returns>This <see cref="BlueprintApiBuilder" /> for further configuration.</returns>
        public static BlueprintApiBuilder AddElasticApm(this BlueprintApiBuilder pipelineBuilder)
        {
            pipelineBuilder.Services.AddSingleton<IApmTool, ElasticApmTool>();

            return pipelineBuilder;
        }
    }
}
