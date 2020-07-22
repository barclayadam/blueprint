using System;
using System.Linq;
using Blueprint;
using Blueprint.Configuration;

// Match the DI container namespace so that Blueprint is immediately discoverable
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlueprintApi(
            this IServiceCollection services,
            Action<BlueprintApiBuilder> configureApi)
        {
            Guard.NotNull(nameof(configureApi), configureApi);

            EnsureNotAlreadySetup(services, typeof(IApiOperationExecutor));

            var apiBuilder = new BlueprintApiBuilder(services);

            configureApi(apiBuilder);

            apiBuilder.Build();

            return services;
        }

        private static void EnsureNotAlreadySetup(IServiceCollection services, Type type)
        {
            if (services.FirstOrDefault(d => d.ServiceType == type) != null)
            {
                throw new InvalidOperationException("Blueprint has already been configured.");
            }
        }
    }
}
