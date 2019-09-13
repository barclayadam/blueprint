using System;
using System.Linq;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Core;

// This is the recommendation from MS for extensions to IServiceCollection to aid discoverability
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlueprintApi(
            this IServiceCollection services,
            Func<BlueprintConfigurer, BlueprintConfigurer> configureApi)
        {
            Guard.NotNull(nameof(configureApi), configureApi);

            CheckBlueprintRegistration(services);

            var blueprintConfigurer = configureApi(new BlueprintConfigurer(services));

            blueprintConfigurer.Build();

            return services;
        }

        public static IServiceCollection AddBlueprintApi(this IServiceCollection services, Action<BlueprintApiOptions> optionsFunc)
        {
            Guard.NotNull(nameof(optionsFunc), optionsFunc);

            CheckBlueprintRegistration(services);

            var options = new BlueprintApiOptions(optionsFunc);
            var blueprintConfigurer = new BlueprintConfigurer(services, options);

            blueprintConfigurer.Build();

            return services;
        }

        private static void CheckBlueprintRegistration(IServiceCollection services)
        {
            if (services.FirstOrDefault(d => d.ServiceType == typeof(IApiOperationExecutor)) != null)
            {
                throw new InvalidOperationException("Blueprint has already been configured.");
            }
        }
    }
}
