using System;
using Hangfire;

namespace Blueprint.Tasks.Hangfire
{
    /// <summary>
    /// Global Configuration extensions.
    /// </summary>
    public static class GlobalConfigurationExtensions
    {
        /// <summary>
        /// Tells global configuration to use the specified StructureMap container as a job activator.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="serviceProvider">The root service provider.</param>
        /// <returns>An instance of <see cref="ServiceProviderJobActivator"/>.</returns>
        /// <exception cref="System.ArgumentNullException">configuration or container.
        /// </exception>
        public static IGlobalConfiguration<ServiceProviderJobActivator> UseServiceProviderActivator(this IGlobalConfiguration configuration, IServiceProvider serviceProvider)
        {
            Guard.NotNull(nameof(configuration), configuration);
            Guard.NotNull(nameof(serviceProvider), serviceProvider);

            return configuration.UseActivator(new ServiceProviderJobActivator(serviceProvider));
        }
    }
}
