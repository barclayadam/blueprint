using Blueprint.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.StructureMap
{
    public static class BlueprintConfigurerExtensions
    {
        public static BlueprintConfigurer AddStructureMap(this BlueprintConfigurer configurer)
        {
            configurer.Services.AddTransient<IInstanceFrameProvider, StructureMapInstanceFrameProvider>();

            return configurer;
        }
    }
}
