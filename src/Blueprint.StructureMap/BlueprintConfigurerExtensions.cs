using Blueprint.Api;
using Blueprint.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.StructureMap
{
    public static class BlueprintConfigurerExtensions
    {
        public static BlueprintApiConfigurer AddStructureMap(this BlueprintApiConfigurer blueprintApiConfigurer)
        {
            blueprintApiConfigurer.Services.AddTransient<IInstanceFrameProvider, StructureMapInstanceFrameProvider>();

            return blueprintApiConfigurer;
        }
    }
}
