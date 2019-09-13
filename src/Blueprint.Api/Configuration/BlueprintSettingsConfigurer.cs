using System.Reflection;

namespace Blueprint.Api.Configuration
{
    public class BlueprintSettingsConfigurer
    {
        private readonly BlueprintApiOptions options;

        public BlueprintSettingsConfigurer(BlueprintApiOptions options)
        {
            this.options = options;
        }

        public void SetAppName(string applicationName)
        {
            options.WithApplicationName(applicationName);
        }

        public void ScanAssemblies(params Assembly[] assemblies)
        {
            options.Scan(assemblies);
        }
    }
}
