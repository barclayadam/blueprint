using System.Security.Claims;
using Blueprint.Authorisation;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Configuration;

/// <summary>
/// A builder for configuring services and options for the authentication pipeline.
/// </summary>
public class BlueprintAuthenticationBuilder
{
    private readonly BlueprintApiBuilder _apiBuilder;

    internal BlueprintAuthenticationBuilder(BlueprintApiBuilder apiBuilder)
    {
        this._apiBuilder = apiBuilder;
    }

    /// <summary>
    /// Specifies the given <see cref="IUserAuthorisationContextFactory" /> as the factory to use when
    /// converting from a <see cref="ClaimsIdentity" /> to a <see cref="IUserAuthorisationContext" />.
    /// </summary>
    /// <returns>This builder.</returns>
    /// <typeparam name="T">The type of factory to register.</typeparam>
    public BlueprintAuthenticationBuilder UseContextLoader<T>() where T : class, IUserAuthorisationContextFactory
    {
        this._apiBuilder.Services.AddScoped<IUserAuthorisationContextFactory, T>();

        return this;
    }
}