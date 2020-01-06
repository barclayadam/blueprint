using System;

namespace Blueprint.Api.Configuration
{
    /// <summary>
    /// Provides extensions to <see cref="BlueprintMiddlewareConfigurer" /> that can be useful in
    /// building up the pipeline used for an API.
    /// </summary>
    public static class PipelineConfigurerExtensions
    {
        /// <summary>
        /// Conditionally executes the given child configurer, which means that for example you could include
        /// certain middleware builders based on a configuration switch, or your environment.
        /// </summary>
        /// <param name="middlewareConfigurer">The middleware configurer.</param>
        /// <param name="include">Whether to include/execute the child action.</param>
        /// <param name="childConfigurer">The action to perform if <paramref name="include"/> is <c>true</c>.</param>
        /// <returns>This middleware configurer.</returns>
        public static BlueprintMiddlewareConfigurer Conditionally(
            this BlueprintMiddlewareConfigurer middlewareConfigurer,
            bool include,
            Action<BlueprintMiddlewareConfigurer> childConfigurer)
        {
            if (include)
            {
                childConfigurer(middlewareConfigurer);
            }

            return middlewareConfigurer;
        }
    }
}
