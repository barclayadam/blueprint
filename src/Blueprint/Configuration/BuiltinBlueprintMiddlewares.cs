﻿using System;
using System.Linq;
using System.Security.Claims;
using Blueprint.Auditing;
using Blueprint.Authorisation;
using Blueprint.Middleware;
using Blueprint.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blueprint.Configuration;

public static class BuiltinBlueprintMiddlewares
{
    public static BlueprintApiBuilder AddAuditing(this BlueprintApiBuilder apiBuilder, Action<BlueprintAuditBuilder> configure)
    {
        apiBuilder.Pipeline(p => p.AddMiddleware<AuditMiddleware>(MiddlewareStage.Setup));

        configure(new BlueprintAuditBuilder(apiBuilder));

        if (apiBuilder.Services.All(s => s.ServiceType != typeof(IAuditor)))
        {
            throw new InvalidOperationException($"An {nameof(IAuditor)} must be configured");
        }

        return apiBuilder;
    }

    /// <summary>
    /// Adds validation middleware, by default adding both Blueprint and DataAnnotations sources.
    /// </summary>
    /// <seealso cref="BlueprintValidationBuilder.UseBlueprintSource" />
    /// <seealso cref="BlueprintValidationBuilder.UseDataAnnotationSource" />
    /// <param name="apiBuilder">The builder to add validation to.</param>
    /// <returns>The builder.</returns>
    public static BlueprintApiBuilder AddValidation(this BlueprintApiBuilder apiBuilder)
    {
        return AddValidation(apiBuilder, o => o.UseBlueprintSource().UseDataAnnotationSource());
    }

    /// <summary>
    /// Adds validation middleware, which will use "validation sources" to handle different types of validation that can be
    /// registered against operations.
    /// </summary>
    /// <seealso cref="BlueprintValidationBuilder.UseBlueprintSource" />
    /// <seealso cref="BlueprintValidationBuilder.UseDataAnnotationSource" />
    /// <param name="apiBuilder">The builder to add validation to.</param>
    /// <param name="configure">An action that will be given an instance of <see cref="BlueprintValidationBuilder" /> to configure the validation
    /// middleware.</param>
    /// <returns>The builder.</returns>
    public static BlueprintApiBuilder AddValidation(this BlueprintApiBuilder apiBuilder, Action<BlueprintValidationBuilder> configure)
    {
        apiBuilder.Pipeline(p => p.AddMiddleware<ValidationMiddlewareBuilder>(MiddlewareStage.Validation));

        configure(new BlueprintValidationBuilder(apiBuilder));

        if (apiBuilder.Services.All(s => s.ServiceType != typeof(IValidationSource)))
        {
            throw new InvalidOperationException($"At least one {nameof(IValidationSource)} must be configured");
        }

        return apiBuilder;
    }

    public static BlueprintApiBuilder AddLogging(this BlueprintApiBuilder apiBuilder)
    {
        apiBuilder.Pipeline(p => p.AddMiddleware<LoggingMiddlewareBuilder>(MiddlewareStage.Setup));

        return apiBuilder;
    }

    /// <summary>
    /// Adds authentication middlewares that will responsible for finding a user through a <see cref="ClaimsIdentity" /> and an
    /// application-specific <see cref="IUserAuthorisationContextFactory"/> for loading a <see cref="IUserAuthorisationContext" /> that will
    /// be available to handlers for authenticated users.
    /// </summary>
    /// <remarks>
    /// Two new middleware builders will be added:
    /// <list type="bullet">
    /// <item><description><see cref="AuthenticationMiddlewareBuilder"/> added to <see cref="MiddlewareStage.Authentication"/>.</description></item>
    /// <item><description><see cref="UserContextLoaderMiddlewareBuilder"/> added to <see cref="MiddlewareStage.Authentication"/>.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="apiBuilder">The builder to add auth to.</param>
    /// <param name="configure">An action to configure authentication pipeline.</param>
    /// <returns>The builder.</returns>
    public static BlueprintApiBuilder AddAuthentication(this BlueprintApiBuilder apiBuilder, Action<BlueprintAuthenticationBuilder> configure)
    {
        apiBuilder.Pipeline(p =>
        {
            p.AddMiddleware<AuthenticationMiddlewareBuilder>(MiddlewareStage.Authentication);
            p.AddMiddleware<UserContextLoaderMiddlewareBuilder>(MiddlewareStage.Authentication);
        });

        configure(new BlueprintAuthenticationBuilder(apiBuilder));

        if (apiBuilder.Services.All(s => s.ServiceType != typeof(IUserAuthorisationContextFactory)))
        {
            throw new InvalidOperationException($"A {nameof(IUserAuthorisationContextFactory)} must be configured");
        }

        return apiBuilder;
    }

    /// <summary>
    /// Adds authorisation middleware and services that ensure authentication users have the correct permissions to execute
    /// operations.
    /// </summary>
    /// <remarks>
    /// New middleware builders will be added:
    /// <list type="bullet">
    /// <item><description><see cref="AuthorisationMiddlewareBuilder"/> added to <see cref="MiddlewareStage.Authorisation"/>.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="apiBuilder">The builder to add auth to.</param>
    /// <returns>The builder.</returns>
    public static BlueprintApiBuilder AddAuthorisation(this BlueprintApiBuilder apiBuilder)
    {
        TryAddAuthorisationServices(apiBuilder.Services);

        apiBuilder.Pipeline(p =>
        {
            p.AddMiddleware<AuthorisationMiddlewareBuilder>(MiddlewareStage.Authorisation);
        });

        return apiBuilder;
    }

    public static void TryAddAuthorisationServices(IServiceCollection services)
    {
        services.TryAddScoped<IApiAuthoriserAggregator, ApiAuthoriserAggregator>();
        services.TryAddScoped<IClaimInspector, ClaimInspector>();

        // We add the authoriser to the enumerable of `IApiAuthoriser`, as well as registering itself as a concrete type so that it can
        // be grabbed from default DI container by it's type (as is required by the code gen)
        void AddAuthoriser<T>() where T : class, IApiAuthoriser
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(IApiAuthoriser), typeof(T)));
            services.TryAddScoped<T, T>();
        }

        AddAuthoriser<ClaimsRequiredApiAuthoriser>();
        AddAuthoriser<MustBeAuthenticatedApiAuthoriser>();
    }
}
