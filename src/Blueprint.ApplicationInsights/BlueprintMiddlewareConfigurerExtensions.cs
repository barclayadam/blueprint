using Blueprint.ApplicationInsights;
using Blueprint.Core.Apm;
using Microsoft.Extensions.DependencyInjection;

// This should be discoverable when configuring without extra namespace imports
// ReSharper disable once CheckNamespace
namespace Blueprint.Api.Configuration
{
    public static class BlueprintMiddlewareConfigurerExtensions
    {
        public static BlueprintMiddlewareConfigurer AddApplicationInsights(this BlueprintMiddlewareConfigurer middlewareConfigurer)
        {
            middlewareConfigurer.Services.AddScoped<IApmTool, ApplicationInsightsApmTool>();

            middlewareConfigurer.AddMiddleware<ApplicationInsightsMiddleware>(MiddlewareStage.Setup);

            return middlewareConfigurer;
        }
    }
}
