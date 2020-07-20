using Blueprint.Apm;
using Blueprint.Apm.DataDog;
using Blueprint.Configuration;

// Match the DI container namespace so that Blueprint is immediately discoverable
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions to <see cref="BlueprintApiBuilder{THost}" /> to add DataDog APM integration.
    /// </summary>
    public static class BlueprintBuilderExtensions
    {
        /// <summary>
        /// Adds DataDog integration to this API, registering an <see cref="IApmTool" /> to allow
        /// tracking dependencies, and middleware that will manage spans.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder to configure.</param>
        /// <typeparam name="THost">The type of host.</typeparam>
        /// <returns>This builder for further configuration.</returns>
        public static BlueprintApiBuilder<THost> AddDataDog<THost>(this BlueprintApiBuilder<THost> pipelineBuilder)
        {
            pipelineBuilder.Services.AddSingleton<IApmTool, DataDogApmTool>();

            return pipelineBuilder;
        }
    }
}
