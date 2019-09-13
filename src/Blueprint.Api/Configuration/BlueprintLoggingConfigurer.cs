using Blueprint.Core;
using Blueprint.Core.Auditing;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Configuration
{
    public class BlueprintLoggingConfigurer
    {
        private readonly IServiceCollection services;

        public BlueprintLoggingConfigurer(IServiceCollection services)
        {
            this.services = services;
        }

        public void None()
        {
            Use(new NullAuditor());
        }

        public void Use(IAuditor auditor)
        {
            Guard.NotNull(nameof(auditor), auditor);

            services.AddTransient(typeof(IAuditor), auditor.GetType());
        }
    }
}
