using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Blueprint;
using Blueprint.Configuration;
using JetBrains.Annotations;

// Match the DI container namespace so that Blueprint is immediately discoverable
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlueprintApi(
        this IServiceCollection services,
        Action<BlueprintApiBuilder> configureApi,
        [CallerFilePath] string callerFilePath = "")
    {
        Guard.NotNull(nameof(configureApi), configureApi);

        EnsureNotAlreadySetup(services);

        var apiBuilder = new BlueprintApiBuilder(services, Assembly.GetCallingAssembly(), callerFilePath);

        configureApi(apiBuilder);

        apiBuilder.Build();

        return services;
    }

    private static void EnsureNotAlreadySetup(IServiceCollection services)
    {
        if (services.FirstOrDefault(d => d.ServiceType == typeof(IApiOperationExecutor)) != null)
        {
            throw new InvalidOperationException("Blueprint has already been configured.");
        }
    }
}