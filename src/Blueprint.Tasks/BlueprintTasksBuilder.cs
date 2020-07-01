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
        /// <param name="serviceCollection">The service collection being configured.</param>
        protected BlueprintTasksBuilder(IServiceCollection serviceCollection)
        {
            Services = serviceCollection;
        }

        /// <summary>
        /// The <see cref="IServiceCollection" /> to configure.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
