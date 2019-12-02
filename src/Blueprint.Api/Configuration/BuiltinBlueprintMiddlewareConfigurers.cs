using System;
using System.Linq;
using Blueprint.Api.Authorisation;
using Blueprint.Api.Formatters;
using Blueprint.Api.Middleware;
using Blueprint.Api.Validation;
using Blueprint.Core.Auditing;
using Blueprint.Core.Authorisation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blueprint.Api.Configuration
{
    public static class BuiltinBlueprintMiddlewareConfigurers
    {
        public static BlueprintMiddlewareConfigurer AddAuditing(this BlueprintMiddlewareConfigurer middlewareConfigurer, Action<BlueprintAuditConfigurer> configure)
        {
            middlewareConfigurer.AddMiddleware<AuditMiddleware>(MiddlewareStage.Setup);

            configure(new BlueprintAuditConfigurer(middlewareConfigurer));

            if (middlewareConfigurer.Services.All(s => s.ServiceType != typeof(IAuditor)))
            {
                throw new InvalidOperationException("An auditor must be configured");
            }

            return middlewareConfigurer;
        }

        /// <summary>
        /// Adds validation middleware, by default adding both Blueprint and DataAnnotations sources.
        /// </summary>
        /// <seealso cref="BlueprintValidationConfigurer.UseBlueprintSource"/>
        /// <seealso cref="BlueprintValidationConfigurer.UseDataAnnotationSource"/>
        /// <param name="middlewareConfigurer">The configurer to add validation to.</param>
        /// <returns>The configurer.</returns>
        public static BlueprintMiddlewareConfigurer AddValidation(this BlueprintMiddlewareConfigurer middlewareConfigurer)
        {
            return AddValidation(middlewareConfigurer, o => o.UseBlueprintSource().UseDataAnnotationSource());
        }

        /// <summary>
        /// Adds validation middleware, which will use "validation sources" to handle different types of validation that can be
        /// registered against <see cref="IApiOperation" /> classes.
        /// </summary>
        /// <seealso cref="BlueprintValidationConfigurer.UseBlueprintSource"/>
        /// <seealso cref="BlueprintValidationConfigurer.UseDataAnnotationSource"/>
        /// <param name="middlewareConfigurer">The configurer to add validation to.</param>
        /// <param name="configure">An action that will be given an instance of <see cref="BlueprintValidationConfigurer"/> to configure the validation
        /// middleware.</param>
        /// <returns>The configurer.</returns>
        public static BlueprintMiddlewareConfigurer AddValidation(this BlueprintMiddlewareConfigurer middlewareConfigurer, Action<BlueprintValidationConfigurer> configure)
        {
            middlewareConfigurer.AddMiddleware<ValidationMiddlewareBuilder>(MiddlewareStage.Validation);

            configure(new BlueprintValidationConfigurer(middlewareConfigurer));

            if (middlewareConfigurer.Services.All(s => s.ServiceType != typeof(IValidationSource)))
            {
                throw new InvalidOperationException("At least one validation source should be specified");
            }

            return middlewareConfigurer;
        }

        public static BlueprintMiddlewareConfigurer AddLogging(this BlueprintMiddlewareConfigurer middlewareConfigurer)
        {
            middlewareConfigurer.AddMiddleware<LoggingMiddlewareBuilder>(MiddlewareStage.Setup);

            return middlewareConfigurer;
        }

        public static BlueprintMiddlewareConfigurer AddHttp(this BlueprintMiddlewareConfigurer middlewareConfigurer)
        {
            middlewareConfigurer.Services.TryAddSingleton<JsonTypeFormatter>();
            middlewareConfigurer.Services.TryAddSingleton<ITypeFormatter, JsonTypeFormatter>();

            middlewareConfigurer.AddMiddleware<HttpMessagePopulationMiddlewareBuilder>(MiddlewareStage.Population);

            return middlewareConfigurer;
        }

        /// <summary>
        /// Adds authentication and authorisation middlewares using the given <see cref="IUserAuthorisationContextFactory"/> type for
        /// loading a <see cref="IUserAuthorisationContext" /> that will be available to handlers for authenticated users.
        /// </summary>
        /// <remarks>
        /// Three new middleware builders will be added:
        /// <list type="bullet">
        /// <item><description><see cref="AuthenticationMiddlewareBuilder"/> added to <see cref="MiddlewareStage.Authentication"/>.</description></item>
        /// <item><description><see cref="UserContextLoaderMiddlewareBuilder"/> added to <see cref="MiddlewareStage.Authorisation"/>.</description></item>
        /// <item><description><see cref="AuthorisationMiddlewareBuilder"/> added to <see cref="MiddlewareStage.Authorisation"/>.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="middlewareConfigurer">The configurer to add auth to.</param>
        /// <typeparam name="T">The type used as a user auth context factory.</typeparam>
        /// <returns>The configurer.</returns>
        public static BlueprintMiddlewareConfigurer AddAuth<T>(this BlueprintMiddlewareConfigurer middlewareConfigurer) where T : class, IUserAuthorisationContextFactory
        {
            middlewareConfigurer.Services.AddScoped<IUserAuthorisationContextFactory, T>();

            TryAddAuthServices(middlewareConfigurer);

            middlewareConfigurer.AddMiddleware<AuthenticationMiddlewareBuilder>(MiddlewareStage.Authentication);
            middlewareConfigurer.AddMiddleware<UserContextLoaderMiddlewareBuilder>(MiddlewareStage.Authorisation);
            middlewareConfigurer.AddMiddleware<AuthorisationMiddlewareBuilder>(MiddlewareStage.Authorisation);

            return middlewareConfigurer;
        }

        public static BlueprintMiddlewareConfigurer AddHateoasLinks(this BlueprintMiddlewareConfigurer middlewareConfigurer)
        {
            // Resource events needs authoriser services to be registered
            TryAddAuthServices(middlewareConfigurer);

            middlewareConfigurer.Services.TryAddSingleton<IApiLinkGenerator, ApiLinkGenerator>();
            middlewareConfigurer.Services.TryAddScoped<IResourceLinkGenerator, EntityOperationResourceLinkGenerator>();

            middlewareConfigurer.AddMiddleware<LinkGeneratorMiddlewareBuilder>(MiddlewareStage.Execution);

            return middlewareConfigurer;
        }

        public static BlueprintMiddlewareConfigurer AddResourceEvents<T>(this BlueprintMiddlewareConfigurer middlewareConfigurer) where T : class, IResourceEventRepository
        {
            middlewareConfigurer.Services.AddScoped<IResourceEventRepository, T>();

            middlewareConfigurer.AddMiddleware<ResourceEventHandlerMiddlewareBuilder>(MiddlewareStage.Execution);

            return middlewareConfigurer;
        }

        private static void TryAddAuthServices(BlueprintMiddlewareConfigurer middlewareConfigurer)
        {
            middlewareConfigurer.Services.TryAddSingleton<IApiAuthoriserAggregator, ApiAuthoriserAggregator>();
            middlewareConfigurer.Services.TryAddSingleton<IClaimInspector, ClaimInspector>();

            middlewareConfigurer.Services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IApiAuthoriser), typeof(ClaimsRequiredApiAuthoriser)));
            middlewareConfigurer.Services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IApiAuthoriser), typeof(MustBeAuthenticatedApiAuthoriser)));
        }
    }
}
