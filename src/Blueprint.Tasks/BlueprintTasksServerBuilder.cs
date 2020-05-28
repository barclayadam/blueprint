using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Tasks
{
    /// <summary>
    /// A builder that is used to configure the server-side of tasks feature.
    /// </summary>
    public class BlueprintTasksServerBuilder : BlueprintTasksBuilder
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="BlueprintTasksServerBuilder" /> class.
        /// </summary>
        /// <param name="serviceCollection">The service collection being configured.</param>
        public BlueprintTasksServerBuilder(IServiceCollection serviceCollection)
            : base(serviceCollection)
        {
        }
    }
}
