using Blueprint.Auditing;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Configuration
{
    public class BlueprintAuditBuilder<THost>
    {
        private readonly BlueprintApiBuilder<THost> apiBuilder;

        public BlueprintAuditBuilder(BlueprintApiBuilder<THost> apiBuilder)
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
