using Blueprint.Auditing;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Configuration
{
    public class BlueprintAuditBuilder
    {
        private readonly BlueprintApiBuilder _apiBuilder;

        public BlueprintAuditBuilder(BlueprintApiBuilder apiBuilder)
        {
            this._apiBuilder = apiBuilder;
        }

        public IServiceCollection Services => this._apiBuilder.Services;

        public void UseAuditor<T>() where T : class, IAuditor
        {
            this.Services.AddScoped<IAuditor, T>();
        }
    }
}
