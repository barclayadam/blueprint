using System;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Configuration
{
    public class BlueprintApiHostBuilder
    {
        private readonly IServiceCollection services;

        private IHostBuilder hostBuilder;

        /// <summary>
        /// Initialises a new instance of the <see cref="BlueprintApiHostBuilder" /> class with the given
        /// <see cref="IServiceCollection" /> in to which all DI registrations will be made.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public BlueprintApiHostBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        /// <summary>
        /// Uses the specified host. This is the entry point to configure a Blueprint API and is used to
        /// separate different hosting methods by allowing extensions to target API builders of a specific
        /// type if it makes sense.
        /// </summary>
        /// <remarks>
        /// This method is <b>NOT</b> typically called directly by client applications, instead it will be called
        /// through an extension method to this class such as Http() that will configure required defaults, conventions
        /// and services needed for that host to execute correctly.
        /// </remarks>
        /// <typeparam name="THost">The type of host to use.</typeparam>
        /// <returns>A new builder to configure the API, typed to the host.</returns>
        public BlueprintApiBuilder<THost> UseHost<THost>() where THost : new()
        {
            this.hostBuilder = new BlueprintApiBuilder<THost>(this.services);

            return (BlueprintApiBuilder<THost>)this.hostBuilder;
        }

        /// <summary>
        /// Uses a "Standalone" host.
        /// </summary>
        /// <returns>A new builder to configure the API, typed to the host.</returns>
        public BlueprintApiBuilder<StandaloneHost> Standalone()
        {
            return this.UseHost<StandaloneHost>();
        }

        internal void Build()
        {
            if (hostBuilder == null)
            {
                throw new InvalidOperationException("No host builder has been configured.");
            }

            hostBuilder.Build();
        }
    }
}
