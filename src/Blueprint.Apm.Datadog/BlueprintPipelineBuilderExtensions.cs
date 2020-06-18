using Blueprint.Apm.DataDog;
using Blueprint.Core.Apm;
using Microsoft.Extensions.DependencyInjection;

// This should be discoverable when configuring without extra namespace imports
// ReSharper disable once CheckNamespace
namespace Blueprint.Api.Configuration
{
    /// <summary>
    /// Extensions to <see cref="BlueprintApiBuilder" /> to add DataDog APM integration.
    /// </summary>
    public static class BlueprintPipelineBuilderExtensions
    {
        /// <summary>
        /// Adds DataDog integration to this API, registering an <see cref="IApmTool" /> to allow
        /// tracking dependencies, and middleware that will manage spans.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder to configure.</param>
        /// <returns>This <see cref="BlueprintApiBuilder" /> for further configuration.</returns>
        public static BlueprintApiBuilder AddDataDog(this BlueprintApiBuilder pipelineBuilder)
        {
            pipelineBuilder.Services.AddScoped<IApmTool, DataDogApmTool>();

            return pipelineBuilder;
        }
    }
}
