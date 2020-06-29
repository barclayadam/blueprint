using Blueprint.Auditing;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Configuration
{
    public class BlueprintAuditBuilder
    {
        private readonly BlueprintPipelineBuilder blueprintPipelineBuilder;

        public BlueprintAuditBuilder(BlueprintPipelineBuilder blueprintPipelineBuilder)
        {
            this.blueprintPipelineBuilder = blueprintPipelineBuilder;
        }

        public IServiceCollection Services => blueprintPipelineBuilder.Services;

        public void UseAuditor<T>() where T : class, IAuditor
        {
            Services.AddScoped<IAuditor, T>();
        }
    }
}
