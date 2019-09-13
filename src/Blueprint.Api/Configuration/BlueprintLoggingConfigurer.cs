using Blueprint.Api.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Configuration
{
    public class BlueprintLoggingConfigurer
    {
        private readonly BlueprintConfigurer blueprintConfigurer;
        private readonly MiddlewareStage? middlewareStage;

        public BlueprintLoggingConfigurer(BlueprintConfigurer blueprintConfigurer, MiddlewareStage? middlewareStage = null)
        {
            this.blueprintConfigurer = blueprintConfigurer;
            this.middlewareStage = middlewareStage;
        }

        public IServiceCollection Services => blueprintConfigurer.Services;

        public void UseNLog()
        {
            blueprintConfigurer.AddMiddlewareBuilderToStage<LoggingMiddlewareBuilder>(middlewareStage ?? MiddlewareStage.OperationChecks);
        }
    }
}
