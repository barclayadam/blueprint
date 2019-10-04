using Blueprint.Core.Auditing;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Configuration
{
    public class BlueprintAuditConfigurer
    {
        private readonly BlueprintApiConfigurer blueprintApiConfigurer;
        private readonly MiddlewareStage? middlewareStage;

        public BlueprintAuditConfigurer(BlueprintApiConfigurer blueprintApiConfigurer, MiddlewareStage? middlewareStage = null)
        {
            this.blueprintApiConfigurer = blueprintApiConfigurer;
            this.middlewareStage = middlewareStage;
        }

        public IServiceCollection Services => blueprintApiConfigurer.Services;

        public void UseAuditor<T>() where T : class, IAuditor
        {
            // blueprintApiConfigurer.AddMiddlewareBuilderToStage<ValidationMiddlewareBuilder>(middlewareStage ?? MiddlewareStage.PreExecute);

            Services.AddScoped<IAuditor, T>();
        }
    }
}
