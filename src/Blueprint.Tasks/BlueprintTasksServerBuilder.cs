using Blueprint.Api.Configuration;

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
        /// <param name="apiBuilder">The builder being configured.</param>
        public BlueprintTasksServerBuilder(BlueprintApiBuilder apiBuilder) : base(apiBuilder)
        {
        }
    }
}
