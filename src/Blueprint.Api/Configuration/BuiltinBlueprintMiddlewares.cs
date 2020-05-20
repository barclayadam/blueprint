using System;
using System.Linq;
using Blueprint.Api.Authorisation;
using Blueprint.Api.Middleware;
using Blueprint.Api.Validation;
using Blueprint.Core.Auditing;
using Blueprint.Core.Authorisation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blueprint.Api.Configuration
{
    public static class BuiltinBlueprintMiddlewares
    {
        public static BlueprintPipelineBuilder AddAuditing(this BlueprintPipelineBuilder pipelineBuilder, Action<BlueprintAuditBuilder> configure)
        {
            pipelineBuilder.AddMiddleware<AuditMiddleware>(MiddlewareStage.Setup);

            configure(new BlueprintAuditBuilder(pipelineBuilder));

            if (pipelineBuilder.Services.All(s => s.ServiceType != typeof(IAuditor)))
            {
                throw new InvalidOperationException("An auditor must be configured");
            }

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds validation middleware, by default adding both Blueprint and DataAnnotations sources.
        /// </summary>
        /// <seealso cref="BlueprintValidationBuilder.UseBlueprintSource"/>
        /// <seealso cref="BlueprintValidationBuilder.UseDataAnnotationSource"/>
        /// <param name="pipelineBuilder">The builder to add validation to.</param>
        /// <returns>The builder.</returns>
        public static BlueprintPipelineBuilder AddValidation(this BlueprintPipelineBuilder pipelineBuilder)
        {
            return AddValidation(pipelineBuilder, o => o.UseBlueprintSource().UseDataAnnotationSource());
        }

        /// <summary>
        /// Adds validation middleware, which will use "validation sources" to handle different types of validation that can be
        /// registered against <see cref="IApiOperation" /> classes.
        /// </summary>
        /// <seealso cref="BlueprintValidationBuilder.UseBlueprintSource"/>
        /// <seealso cref="BlueprintValidationBuilder.UseDataAnnotationSource"/>
        /// <param name="pipelineBuilder">The builder to add validation to.</param>
        /// <param name="configure">An action that will be given an instance of <see cref="BlueprintValidationBuilder"/> to configure the validation
        /// middleware.</param>
        /// <returns>The builder.</returns>
        public static BlueprintPipelineBuilder AddValidation(this BlueprintPipelineBuilder pipelineBuilder, Action<BlueprintValidationBuilder> configure)
        {
            pipelineBuilder.AddMiddleware<ValidationMiddlewareBuilder>(MiddlewareStage.Validation);

            configure(new BlueprintValidationBuilder(pipelineBuilder));

            if (pipelineBuilder.Services.All(s => s.ServiceType != typeof(IValidationSource)))
            {
                throw new InvalidOperationException("At least one validation source should be specified");
            }

            return pipelineBuilder;
        }

        public static BlueprintPipelineBuilder AddLogging(this BlueprintPipelineBuilder pipelineBuilder)
        {
            pipelineBuilder.AddMiddleware<LoggingMiddlewareBuilder>(MiddlewareStage.Setup);

            return pipelineBuilder;
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
        /// <param name="pipelineBuilder">The builder to add auth to.</param>
        /// <typeparam name="T">The type used as a user auth context factory.</typeparam>
        /// <returns>The builder.</returns>
        public static BlueprintPipelineBuilder AddAuth<T>(this BlueprintPipelineBuilder pipelineBuilder) where T : class, IUserAuthorisationContextFactory
        {
            pipelineBuilder.Services.AddScoped<IUserAuthorisationContextFactory, T>();

            TryAddAuthServices(pipelineBuilder);

            pipelineBuilder.AddMiddleware<AuthenticationMiddlewareBuilder>(MiddlewareStage.Authentication);
            pipelineBuilder.AddMiddleware<UserContextLoaderMiddlewareBuilder>(MiddlewareStage.Authorisation);
            pipelineBuilder.AddMiddleware<AuthorisationMiddlewareBuilder>(MiddlewareStage.Authorisation);

            return pipelineBuilder;
        }

        public static void TryAddAuthServices(BlueprintPipelineBuilder pipelineBuilder)
        {
            pipelineBuilder.Services.TryAddSingleton<IApiAuthoriserAggregator, ApiAuthoriserAggregator>();
            pipelineBuilder.Services.TryAddSingleton<IClaimInspector, ClaimInspector>();

            // It is expected that this is overriden, for example by AddHttp
            pipelineBuilder.Services.TryAddSingleton<IClaimsIdentityProvider, NullClaimsIdentityProvider>();

            // We add the authoriser to the enumerable of `IApiAuthoriser`, as well as registering itself as a concrete type so that it can
            // be grabbed from default DI container by it's type (as is required by the code gen)
            void AddAuthoriser<T>() where T : class, IApiAuthoriser
            {
                pipelineBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IApiAuthoriser), typeof(T)));
                pipelineBuilder.Services.TryAddSingleton<T, T>();
            }

            AddAuthoriser<ClaimsRequiredApiAuthoriser>();
            AddAuthoriser<MustBeAuthenticatedApiAuthoriser>();
        }
    }
}
