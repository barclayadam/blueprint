using Blueprint.Core.Auditing;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Configuration
{
    public class BlueprintAuditConfigurer
    {
        private readonly BlueprintPipelineBuilder blueprintApiConfigurer;

        public BlueprintAuditConfigurer(BlueprintPipelineBuilder blueprintApiConfigurer)
        {
            this.blueprintApiConfigurer = blueprintApiConfigurer;
        }

        public IServiceCollection Services => blueprintApiConfigurer.Services;

        public void UseAuditor<T>() where T : class, IAuditor
        {
            Services.AddScoped<IAuditor, T>();
        }
    }
}
