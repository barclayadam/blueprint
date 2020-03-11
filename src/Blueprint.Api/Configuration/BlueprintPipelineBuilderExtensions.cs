using System;

namespace Blueprint.Api.Configuration
{
    /// <summary>
    /// Provides extensions to <see cref="BlueprintPipelineBuilder" /> that can be useful in
    /// building up the pipeline used for an API.
    /// </summary>
    public static class BlueprintPipelineBuilderExtensions
    {
        /// <summary>
        /// Conditionally executes the given action, which means that for example you could include
        /// certain middleware builders based on a configuration switch, or your environment.
        /// </summary>
        /// <param name="pipelineBuilder">The middleware builder.</param>
        /// <param name="include">Whether to include/execute the child action.</param>
        /// <param name="childBuilderAction">The action to perform if <paramref name="include"/> is <c>true</c>.</param>
        /// <returns>This middleware builder.</returns>
        public static BlueprintPipelineBuilder Conditionally(
            this BlueprintPipelineBuilder pipelineBuilder,
            bool include,
            Action<BlueprintPipelineBuilder> childBuilderAction)
        {
            if (include)
            {
                childBuilderAction(pipelineBuilder);
            }

            return pipelineBuilder;
        }
    }
}
