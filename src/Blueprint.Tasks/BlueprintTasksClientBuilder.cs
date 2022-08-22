using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Tasks;

/// <summary>
/// A builder that is used to configure the client-side of tasks feature.
/// </summary>
public class BlueprintTasksClientBuilder : BlueprintTasksBuilder
{
    /// <summary>
    /// Initialises a new instance of the <see cref="BlueprintTasksClientBuilder" /> class.
    /// </summary>
    /// <param name="serviceCollection">The service collection being configured.</param>
    public BlueprintTasksClientBuilder(IServiceCollection serviceCollection)
        : base(serviceCollection)
    {
    }
}