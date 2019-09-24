using Blueprint.Api.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Configuration
{
    public class BlueprintLoggingConfigurer
    {
        private readonly BlueprintApiConfigurer blueprintApiConfigurer;
        private readonly MiddlewareStage? middlewareStage;

        public BlueprintLoggingConfigurer(BlueprintApiConfigurer blueprintApiConfigurer, MiddlewareStage? middlewareStage = null)
        {
            this.blueprintApiConfigurer = blueprintApiConfigurer;
            this.middlewareStage = middlewareStage;
        }

        public IServiceCollection Services => blueprintApiConfigurer.Services;

        public void UseNLog()
        {
            blueprintApiConfigurer.AddMiddlewareBuilderToStage<LoggingMiddlewareBuilder>(middlewareStage ?? MiddlewareStage.OperationChecks);
        }
    }
}
