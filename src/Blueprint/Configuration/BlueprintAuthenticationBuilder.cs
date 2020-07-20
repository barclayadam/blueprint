using System.Security.Claims;
using Blueprint.Authorisation;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Configuration
{
    /// <summary>
    /// A builder for configuring services and options for the authentication pipeline.
    /// </summary>
    /// <typeparam name="THost">The host-type of the API builder.</typeparam>
    public class BlueprintAuthenticationBuilder<THost>
    {
        private readonly BlueprintApiBuilder<THost> apiBuilder;

        internal BlueprintAuthenticationBuilder(BlueprintApiBuilder<THost> apiBuilder)
        {
            this.apiBuilder = apiBuilder;
        }

        /// <summary>
        /// Specifies the given <see cref="IUserAuthorisationContextFactory" /> as the factory to use when
        /// converting from a <see cref="ClaimsIdentity" /> to a <see cref="IUserAuthorisationContext" />.
        /// </summary>
        /// <returns>This builder.</returns>
        /// <typeparam name="T">The type of factory to register.</typeparam>
        public BlueprintAuthenticationBuilder<THost> UseContextLoader<T>() where T : class, IUserAuthorisationContextFactory
        {
            apiBuilder.Services.AddScoped<IUserAuthorisationContextFactory, T>();

            return this;
        }
    }
}
