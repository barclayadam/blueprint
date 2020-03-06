using Blueprint.Api;
using Blueprint.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.StructureMap
{
    public static class BlueprintConfigurerExtensions
    {
        /// <summary>
        /// Adds StructureMap support to Blueprint by registering an <see cref="InstanceFrameProvider" /> that understands
        /// the implicit registration nature of StructureMap.
        /// </summary>
        /// <param name="blueprintApiBuilder">The configurer.</param>
        /// <returns>The configurer for further customisation.</returns>
        public static BlueprintApiBuilder AddStructureMap(this BlueprintApiBuilder blueprintApiBuilder)
        {
            blueprintApiBuilder.Services.AddTransient<InstanceFrameProvider, StructureMapInstanceFrameProvider>();

            return blueprintApiBuilder;
        }
    }
}
