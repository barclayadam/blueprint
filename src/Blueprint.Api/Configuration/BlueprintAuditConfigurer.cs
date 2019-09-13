using Blueprint.Core;
using Blueprint.Core.Auditing;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Configuration
{
    public class BlueprintAuditConfigurer
    {
        private readonly BlueprintConfigurer blueprintConfigurer;

        public BlueprintAuditConfigurer(BlueprintConfigurer blueprintConfigurer, MiddlewareStage? middlewareStage = null)
        {
            this.blueprintConfigurer = blueprintConfigurer;

            //blueprintConfigurer.AddMiddlewareBuilderToStage<ValidationMiddlewareBuilder>(middlewareStage ?? MiddlewareStage.PreExecute);
        }

        public void None()
        {
            Use(new NullAuditor());
        }

        public void Use(IAuditor auditor)
        {
            Guard.NotNull(nameof(auditor), auditor);

            blueprintConfigurer.Services.AddTransient(typeof(IAuditor), auditor.GetType());
        }
    }
}
