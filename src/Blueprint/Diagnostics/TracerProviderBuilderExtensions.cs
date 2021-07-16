using System;
using Blueprint.Diagnostics;

// ReSharper disable once CheckNamespace
namespace OpenTelemetry.Trace
{
    /// <summary>
    /// Extension methods to simplify registering of Blueprint operation instrumentation.
    /// </summary>
    public static class TracerProviderBuilderExtensions
    {
        /// <summary>
        /// Enables the automatic data collection for Blueprint and all modules.
        /// </summary>
        /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
        /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
        public static TracerProviderBuilder AddBlueprintInstrumentation(
            this TracerProviderBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            // Enables all Blueprint activities (Blueprint or Blueprint.[Module]).
            builder.AddSource(BlueprintActivitySource.ActivitySourceName + "*");

            return builder;
        }
    }
}
