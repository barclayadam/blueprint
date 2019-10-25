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

        public static BlueprintMiddlewareConfigurer AddValidation(this BlueprintMiddlewareConfigurer middlewareConfigurer)
        {
            return AddValidation(middlewareConfigurer, o => o.UseBlueprintSource().UseDataAnnotationSource());
        }

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

        public static BlueprintMiddlewareConfigurer UseAuth(this BlueprintMiddlewareConfigurer middlewareConfigurer)
        {
            middlewareConfigurer.Services.TryAddScoped<IApiAuthoriserAggregator, ApiAuthoriserAggregator>();
            middlewareConfigurer.Services.TryAddScoped<IClaimInspector, ClaimInspector>();

            middlewareConfigurer.Services.AddScoped<IApiAuthoriser, ClaimsRequiredApiAuthoriser>();
            middlewareConfigurer.Services.AddScoped<IApiAuthoriser, MustBeAuthenticatedApiAuthoriser>();

            middlewareConfigurer.AddMiddleware<AuthenticationMiddlewareBuilder>(MiddlewareStage.Authentication);
            middlewareConfigurer.AddMiddleware<UserContextLoaderMiddlewareBuilder>(MiddlewareStage.Authorisation);
            middlewareConfigurer.AddMiddleware<AuthorisationMiddlewareBuilder>(MiddlewareStage.Authorisation);

            return middlewareConfigurer;
        }

        public static BlueprintMiddlewareConfigurer AddHateoasLinks(this BlueprintMiddlewareConfigurer middlewareConfigurer)
        {
            middlewareConfigurer.Services.TryAddScoped<IResourceLinkGenerator, EntityOperationResourceLinkGenerator>();

            middlewareConfigurer.AddMiddleware<LinkGeneratorMiddlewareBuilder>(MiddlewareStage.Execution);

            return middlewareConfigurer;
        }

        public static BlueprintMiddlewareConfigurer AddResourceEvents(this BlueprintMiddlewareConfigurer middlewareConfigurer)
        {
            middlewareConfigurer.Services.TryAddScoped<IApiAuthoriserAggregator, ApiAuthoriserAggregator>();
            middlewareConfigurer.Services.TryAddScoped<IClaimInspector, ClaimInspector>();

            middlewareConfigurer.Services.AddScoped<IApiAuthoriser, ClaimsRequiredApiAuthoriser>();
            middlewareConfigurer.Services.AddScoped<IApiAuthoriser, MustBeAuthenticatedApiAuthoriser>();

            middlewareConfigurer.AddMiddleware<ResourceEventHandlerMiddlewareBuilder>(MiddlewareStage.Execution);

            return middlewareConfigurer;
        }
    }
}
