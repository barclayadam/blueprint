using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api
{
    public class BlueprintConfigurer
    {
        public BlueprintConfigurer(IServiceCollection serviceCollection, BlueprintApiOptions options)
        {
            Services = serviceCollection;
            Options = options;
        }

        public IServiceCollection Services { get; }

        public BlueprintApiOptions Options { get; }
    }
}
