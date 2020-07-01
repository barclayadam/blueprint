using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint;
using Blueprint.Configuration;
using Blueprint.Middleware;

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
