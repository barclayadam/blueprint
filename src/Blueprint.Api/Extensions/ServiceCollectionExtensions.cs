using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Core;
using Blueprint.Core.Tasks;

// This is the recommendation from MS for extensions to IServiceCollection to aid discoverability
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlueprintApi(
            this IServiceCollection services,
            Action<BlueprintApiConfigurer> configureApi)
        {
            Guard.NotNull(nameof(configureApi), configureApi);

            EnsureNotAlreadySetup(services, typeof(IApiOperationExecutor));

            var apiConfigurer = new BlueprintApiConfigurer(services);

            configureApi(apiConfigurer);

            apiConfigurer.Build();

            return services;
        }

        public static IServiceCollection AddBlueprintTasks(
            this IServiceCollection services,
            Func<BlueprintTasksConfigurer, BlueprintTasksConfigurer> configureTasks)
        {
            EnsureNotAlreadySetup(services, typeof(IBackgroundTaskScheduler));

            configureTasks(new BlueprintTasksConfigurer(services));

            return services;
        }

        internal static IServiceCollection AddApiOperationHandlers(
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

        private static void EnsureNotAlreadySetup(IServiceCollection services, Type type)
        {
            if (services.FirstOrDefault(d => d.ServiceType == type) != null)
            {
                throw new InvalidOperationException("Blueprint has already been configured.");
            }
        }
    }
}
