using Blueprint.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Tasks
{
    /// <summary>
    /// Base class of the client and server task builders, provided for common builder methods to
    /// be added to if they apply equally to server and client side configurations.
    /// </summary>
    public abstract class BlueprintTasksBuilder
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="BlueprintTasksClientBuilder" /> class.
        /// </summary>
        /// <param name="apiBuilder">The builder being configured.</param>
        protected BlueprintTasksBuilder(BlueprintApiBuilder apiBuilder)
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
