using Blueprint.StructureMap;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace For discoverability we add to existing namespace
namespace Blueprint.Configuration
{
    /// <summary>
    /// Extensions to <see cref="BlueprintApiBuilder" /> for installing StructureMap.
    /// </summary>
    public static class BlueprintApiBuilderExtensions
    {
        /// <summary>
        /// Adds StructureMap support to Blueprint by registering an <see cref="InstanceFrameProvider" /> that understands
        /// the implicit registration nature of StructureMap.
        /// </summary>
        /// <param name="blueprintApiBuilder">The builder.</param>
        /// <returns>The builder for further customisation.</returns>
        public static BlueprintApiBuilder AddStructureMap(this BlueprintApiBuilder blueprintApiBuilder)
        {
            blueprintApiBuilder.Services.AddTransient<InstanceFrameProvider, StructureMapInstanceFrameProvider>();

            return blueprintApiBuilder;
        }
    }
}
