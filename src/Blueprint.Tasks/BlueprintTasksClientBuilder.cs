using Blueprint.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Tasks
{
    /// <summary>
    /// A builder that is used to configure the client-side of tasks feature.
    /// </summary>
    public class BlueprintTasksClientBuilder
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="BlueprintTasksClientBuilder" /> class.
        /// </summary>
        /// <param name="apiBuilder">The builder being configured.</param>
        public BlueprintTasksClientBuilder(BlueprintApiBuilder apiBuilder)
        {
            ApiBuilder = apiBuilder;
            Services = apiBuilder.Services;
        }

        /// <summary>
        /// The <see cref="BlueprintApiBuilder" /> being configured.
        /// </summary>
        public BlueprintApiBuilder ApiBuilder { get; }

        /// <summary>
        /// The <see cref="IServiceCollection" /> to configure.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
