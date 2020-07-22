using System;

namespace Blueprint.Configuration
{
    /// <summary>
    /// Provides extensions to <see cref="BlueprintApiBuilder" /> that can be useful in
    /// building up the pipeline used for an API.
    /// </summary>
    public static class BlueprintBuilderExtensions
    {
        /// <summary>
        /// Conditionally executes the given child action, which means that for example you could include
        /// certain middleware builders based on a configuration switch, or your environment.
        /// </summary>
        /// <param name="apiBuilder">The Api builder.</param>
        /// <param name="include">Whether to include/execute the child action.</param>
        /// <param name="action">The action to perform if <paramref name="include"/> is <c>true</c>.</param>
        /// <returns>This builder.</returns>
        public static BlueprintApiBuilder Conditionally(
            this BlueprintApiBuilder apiBuilder,
            bool include,
            Action<BlueprintApiBuilder> action)
        {
            if (include)
            {
                action(apiBuilder);
            }

            return apiBuilder;
        }

        /// <summary>
        /// Conditionally executes the given action, which means that for example you could include
        /// certain middleware builders based on a configuration switch, or your environment.
        /// </summary>
        /// <param name="pipelineBuilder">The middleware builder.</param>
        /// <param name="include">Whether to include/execute the child action.</param>
        /// <param name="action">The action to perform if <paramref name="include"/> is <c>true</c>.</param>
        /// <returns>This middleware builder.</returns>
        public static PipelineBuilder Conditionally(
            this PipelineBuilder pipelineBuilder,
            bool include,
            Action<PipelineBuilder> action)
        {
            if (include)
            {
                action(pipelineBuilder);
            }

            return pipelineBuilder;
        }
    }
}
