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

        public void SetAppName(string appName)
        {
            options.WithAppName(appName);
        }

        public void AssembliesToScanForOperations(params Assembly[] assemblies)
        {
            options.ScanForOperations(assemblies);
        }
    }
}
