using System;
using System.Collections.Generic;
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
            Func<BlueprintApiConfigurer, BlueprintApiConfigurer> configureApi)
        {
            Guard.NotNull(nameof(configureApi), configureApi);

            CheckBlueprintRegistration(services);

            var blueprintApiConfigurer = configureApi(new BlueprintApiConfigurer(services));

            blueprintApiConfigurer.Build();

            return services;
        }

        public static IServiceCollection AddBlueprintApi(
            this IServiceCollection services,
            Action<BlueprintApiOptions> optionsFunc)
        {
            Guard.NotNull(nameof(optionsFunc), optionsFunc);

            CheckBlueprintRegistration(services);

            var options = new BlueprintApiOptions(optionsFunc);
            var blueprintApiConfigurer = new BlueprintApiConfigurer(services, options);

            blueprintApiConfigurer.Build();

            return services;
        }

        public static IServiceCollection AddBlueprintTasks(
            this IServiceCollection services,
            Func<BlueprintTasksConfigurer, BlueprintTasksConfigurer> configureTasks)
        {
            CheckBlueprintRegistration(services);

            configureTasks(new BlueprintTasksConfigurer(services));

            return services;
        }

        public static IServiceCollection AddApiOperationHandlers(
            this IServiceCollection services,
            IEnumerable<ApiOperationDescriptor> operations)
        {
            var missingApiOperationHandlers = new List<ApiOperationDescriptor>();

            if (!operations.Any())
            {
                // TODO
            }

            foreach (var operation in operations)
            {
                var apiOperationHandlerType = typeof(IApiOperationHandler<>).MakeGenericType(operation.OperationType);
                var apiOperationHandler = FindApiOperationHandler(operation, apiOperationHandlerType);

                if (apiOperationHandler == null)
                {
                    // We will search for anything that has already been registered before adding to the "not registered"
                    // pile
                    if (services.All(d => apiOperationHandlerType.IsAssignableFrom(d.ServiceType)))
                    {
                        missingApiOperationHandlers.Add(operation);
                    }

                    continue;
                }

                services.AddScoped(apiOperationHandlerType, apiOperationHandler);
            }

            if (missingApiOperationHandlers.Any())
            {
                throw new MissingApiOperationHandlerException(missingApiOperationHandlers.ToArray());
            }

            return services;
        }

        private static Type FindApiOperationHandler(ApiOperationDescriptor apiOperationDescriptor, Type apiOperationHandlerType)
        {
            return apiOperationDescriptor.OperationType.Assembly.GetExportedTypes().SingleOrDefault(apiOperationHandlerType.IsAssignableFrom);
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
