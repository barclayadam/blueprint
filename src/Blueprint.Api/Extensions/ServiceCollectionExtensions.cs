using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Api.Middleware;
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
            Action<BlueprintApiBuilder> configureApi)
        {
            Guard.NotNull(nameof(configureApi), configureApi);

            EnsureNotAlreadySetup(services, typeof(IApiOperationExecutor));

            var apiConfigurer = new BlueprintApiBuilder(services);

            configureApi(apiConfigurer);

            apiConfigurer.Build();

            return services;
        }

        internal static IServiceCollection AddApiOperationHandlers(
            this IServiceCollection services,
            List<ApiOperationDescriptor> operations)
        {
            var allFound = new List<IOperationExecutorBuilder>();
            var scanners = new IOperationExecutorBuilderScanner[]
            {
                new ApiOperationHandlerExecutorBuilderScanner(),
                new ApiOperationInClassConventionExecutorBuilderScanner(),
            };

            var problems = new List<string>();

            foreach (var scanner in scanners)
            {
                foreach (var found in scanner.FindHandlers(services, operations))
                {
                    var existing = allFound.Where(e => e.Operation == found.Operation).ToList();

                    if (existing.Any())
                    {
                        var all = string.Join(", ", existing.Select(e => e.ToString()));

                        problems.Add($"Multiple handlers have been found for the operation {found}: {all} ");
                    }

                    allFound.Add(found);
                }
            }

            var missing = operations.Where(o => allFound.All(f => f.Operation != o)).ToList();

            if (missing.Any())
            {
                throw new MissingApiOperationHandlerException(missing.ToArray());
            }

            if (problems.Any())
            {
                throw new InvalidOperationException(string.Join("\n", problems));
            }

            services.AddSingleton(allFound);

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
