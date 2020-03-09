using Blueprint.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Tasks
{
    public class BlueprintTasksClientBuilder
    {
        public BlueprintTasksClientBuilder(BlueprintApiBuilder apiBuilder)
        {
            ApiBuilder = apiBuilder;
            Services = apiBuilder.Services;
        }

        public BlueprintApiBuilder ApiBuilder { get; }

        public IServiceCollection Services { get; }
    }

    public class BlueprintTasksServerBuilder
    {
        public BlueprintTasksServerBuilder(BlueprintApiBuilder apiBuilder)
        {
            ApiBuilder = apiBuilder;
            Services = apiBuilder.Services;
        }

        public BlueprintApiBuilder ApiBuilder { get; }

        public IServiceCollection Services { get; }
    }
}
