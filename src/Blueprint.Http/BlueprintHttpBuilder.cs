using System;
using Blueprint.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A builder that exposes methods to modify the behaviour of the HTTP functionality of
/// Blueprint, including changing JSON settings.
/// </summary>
public class BlueprintHttpBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlueprintHttpBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    internal BlueprintHttpBuilder(IServiceCollection services)
    {
        this._services = services;
    }

    /// <summary>
    /// Configures the default JSON settings (System.Text.Json) that Blueprint uses for (de)serialisation of
    /// JSON request and response bodies.
    /// </summary>
    /// <param name="configure">The action to call.</param>
    /// <returns>This builder.</returns>
    public BlueprintHttpBuilder Json(Action<BlueprintJsonOptions> configure)
    {
        this._services.Configure(configure);

        return this;
    }

    /// <summary>
    /// Configures the <see cref="BlueprintHttpOptions" /> options instance.
    /// </summary>
    /// <param name="configure">The action to call.</param>
    /// <returns>This builder.</returns>
    public BlueprintHttpBuilder Options(Action<BlueprintHttpOptions> configure)
    {
        this._services.Configure(configure);

        return this;
    }
}