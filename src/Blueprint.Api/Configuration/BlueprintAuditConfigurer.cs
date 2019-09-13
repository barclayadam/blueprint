using Blueprint.Core.Auditing;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Configuration
{
    public class BlueprintAuditConfigurer
    {
        private readonly BlueprintConfigurer blueprintConfigurer;
        private readonly MiddlewareStage? middlewareStage;

        public BlueprintAuditConfigurer(BlueprintConfigurer blueprintConfigurer, MiddlewareStage? middlewareStage = null)
        {
            this.blueprintConfigurer = blueprintConfigurer;
            this.middlewareStage = middlewareStage;
        }

        public IServiceCollection Services => blueprintConfigurer.Services;

        public void UseAuditor<T>() where T : class, IAuditor
        {
            //blueprintConfigurer.AddMiddlewareBuilderToStage<ValidationMiddlewareBuilder>(middlewareStage ?? MiddlewareStage.PreExecute);

            Services.AddScoped<IAuditor, T>();
        }
    }
}
