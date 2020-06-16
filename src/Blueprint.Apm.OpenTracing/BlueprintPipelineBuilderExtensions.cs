using Blueprint.Apm.OpenTracing;
using Blueprint.Core.Apm;
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
        /// Adds OpenTracing integration to this API, registering an <see cref="IApmTool" /> to allow
        /// tracking dependencies, and middleware that will create new <see cref="span"/>
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder to configure.</param>
        /// <returns>This <see cref="BlueprintApiBuilder" /> for further configuration.</returns>
        public static BlueprintApiBuilder AddOpenTracing(this BlueprintApiBuilder pipelineBuilder)
        {
            pipelineBuilder.Services.AddScoped<IApmTool, OpenTracingApmTool>();

            pipelineBuilder.Pipeline(p => p.AddMiddleware<OpenTracingApmMiddleware>(MiddlewareStage.Setup));

            return pipelineBuilder;
        }
    }
}
