using Blueprint.Apm;
using Blueprint.Apm.Elastic;
using Blueprint.Configuration;

// Match the DI container namespace so that Blueprint is immediately discoverable
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions to <see cref="BlueprintApiBuilder" /> to add Elastic APM integration.
    /// </summary>
    public static class BlueprintBuilderExtensions
    {
        /// <summary>
        /// Adds Elastic APM integration to this API, registering an <see cref="IApmTool" /> to allow
        /// tracking dependencies using Elastic APM.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder to configure.</param>
        /// <returns>This builder for further configuration.</returns>
        public static BlueprintApiBuilder AddElasticApm(this BlueprintApiBuilder pipelineBuilder)
        {
            pipelineBuilder.Services.AddSingleton<IApmTool, ElasticApmTool>();

            return pipelineBuilder;
        }
    }
}
