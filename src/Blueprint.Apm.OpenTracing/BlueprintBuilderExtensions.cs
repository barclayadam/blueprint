using Blueprint.Apm;
using Blueprint.Apm.OpenTracing;
using Blueprint.Configuration;

// Match the DI container namespace so that Blueprint is immediately discoverable
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions to <see cref="BlueprintApiBuilder" /> to add OpenTracing APM integration.
    /// </summary>
    public static class BlueprintBuilderExtensions
    {
        /// <summary>
        /// Adds OpenTracing integration to this API, registering an <see cref="IApmTool" /> to allow
        /// tracking dependencies using the OpenTracing library.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder to configure.</param>
        /// <returns>This builder for further configuration.</returns>
        public static BlueprintApiBuilder AddOpenTracing(this BlueprintApiBuilder pipelineBuilder)
        {
            pipelineBuilder.Services.AddSingleton<IApmTool, OpenTracingApmTool>();

            return pipelineBuilder;
        }
    }
}
