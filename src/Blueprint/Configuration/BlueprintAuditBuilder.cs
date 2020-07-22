using Blueprint.Auditing;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Configuration
{
    public class BlueprintAuditBuilder
    {
        private readonly BlueprintApiBuilder apiBuilder;

        public BlueprintAuditBuilder(BlueprintApiBuilder apiBuilder)
        {
            this.apiBuilder = apiBuilder;
        }

        public IServiceCollection Services => apiBuilder.Services;

        public void UseAuditor<T>() where T : class, IAuditor
        {
            Services.AddScoped<IAuditor, T>();
        }
    }
}
