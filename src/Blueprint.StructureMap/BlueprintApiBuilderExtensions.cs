using Blueprint.StructureMap;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace For discoverability we add to existing namespace
namespace Blueprint.Configuration
{
    /// <summary>
    /// Extensions to <see cref="BlueprintApiBuilder{THost}" /> for using StructureMap as a dependency container, matching
    /// the rules of SM when determining what can be loaded from the DI container.
    /// </summary>
    public static class BlueprintApiBuilderExtensions
    {
        /// <summary>
        /// Adds StructureMap support to Blueprint by registering an <see cref="InstanceFrameProvider" /> that understands
        /// the implicit registration nature of StructureMap.
        /// </summary>
        /// <param name="blueprintApiBuilder">The builder.</param>
        /// <typeparam name="THost">The type of host.</typeparam>
        /// <returns>The builder for further customisation.</returns>
        public static BlueprintApiBuilder<THost> AddStructureMap<THost>(this BlueprintApiBuilder<THost> blueprintApiBuilder)
        {
            blueprintApiBuilder.Services.AddTransient<InstanceFrameProvider, StructureMapInstanceFrameProvider>();

            return blueprintApiBuilder;
        }
    }
}
