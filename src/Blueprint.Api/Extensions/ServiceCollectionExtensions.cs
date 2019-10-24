using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Blueprint.Api;
using Blueprint.Api.Authorisation;
using Blueprint.Api.Errors;
using Blueprint.Api.Formatters;
using Blueprint.Api.Infrastructure;
using Blueprint.Api.Validation;
using Blueprint.Compiler;
using Blueprint.Core.Apm;
using Blueprint.Core.Authorisation;
using Blueprint.Core.Caching;
using Blueprint.Core.Errors;
using Blueprint.Core.Tracing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

// This is the recommendation from MS for extensions to IServiceCollection to aid discoverability
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static BlueprintConfigurer AddBlueprintApi(this IServiceCollection services, Action<BlueprintApiOptions> optionsFunc)
        {
            services.AddLogging();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var options = new BlueprintApiOptions(optionsFunc);

            // Register the collection that built the service provider so that the code generation can inspect the registrations and
            // generate better code (i.e. inject singleton services in to the pipeline executor instead of getting them at operation execution time)
            services.AddSingleton(services);

            services.AddSingleton(options);
            services.AddSingleton(options.Model);
            services.AddSingleton<ApiConfiguration>();
            services.AddSingleton<ApiLinkGenerator>();
            services.AddSingleton<AssemblyGenerator>();
            services.AddSingleton<ToFileCompileStrategy>();
            services.AddSingleton<InMemoryOnlyCompileStrategy>();
            services.AddSingleton<AssemblyGenerator>();
            services.AddSingleton<IApiOperationExecutor>(s => new ApiOperationExecutorBuilder(s.GetRequiredService<ILogger<ApiOperationExecutorBuilder>>()).Build(options, s));

            services.TryAddScoped<IErrorLogger, ErrorLogger>();

            // Authentication / Authorisation
            services.TryAddScoped<IApiAuthoriserAggregator, ApiAuthoriserAggregator>();
            services.TryAddScoped<IClaimInspector, ClaimInspector>();

            services.AddScoped<IApiAuthoriser, ClaimsRequiredApiAuthoriser>();
            services.AddScoped<IApiAuthoriser, MustBeAuthenticatedApiAuthoriser>();

            // Validation
            services.TryAddSingleton<IValidationSource, DataAnnotationsValidationSource>();
            services.TryAddSingleton<IValidationSource, BlueprintValidationSource>();
            services.TryAddSingleton<IValidationSourceBuilder, DataAnnotationsValidationSourceBuilder>();
            services.TryAddSingleton<IValidationSourceBuilder, BlueprintValidationSourceBuilder>();
            services.TryAddSingleton<IValidator, BlueprintValidator>();

            // Cache
            services.TryAddSingleton<ICache, Cache>();
            services.TryAddSingleton(MemoryCache.Default);
            services.TryAddSingleton<IExceptionFilter, BasicExceptionFilter>();

            // IoC
            services.TryAddTransient<IInstanceFrameProvider, DefaultInstanceFrameProvider>();

            // Formatters
            services.TryAddSingleton<JsonTypeFormatter>();
            services.TryAddSingleton<ITypeFormatter, JsonTypeFormatter>();

            // Linking
            services.TryAddScoped<IResourceLinkGenerator, EntityOperationResourceLinkGenerator>();

            // Random infrastructure
            services.TryAddScoped<IVersionInfoProvider, NulloVersionInfoProvider>();
            services.TryAddScoped<IApmTool, NullApmTool>();

            services.TryAddScoped(_ => ArrayPool<char>.Shared);
            services.TryAddScoped(_ => ArrayPool<byte>.Shared);

            services.AddSingleton<IHttpRequestStreamReaderFactory, MemoryPoolHttpRequestStreamReaderFactory>();
            services.AddSingleton<IHttpResponseStreamWriterFactory, MemoryPoolHttpResponseStreamWriterFactory>();

            // Tasks
            // services.AddScoped<IBackgroundTaskScheduler, BackgroundTaskScheduler>();
            // Decorate: services.AddScoped<IBackgroundTaskScheduleProvider, ActivityTrackingBackgroundTaskScheduleProvider<Hangfire>>();

            var missingApiOperationHandlers = new List<ApiOperationDescriptor>();

            foreach (var operation in options.Model.Operations)
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

            return new BlueprintConfigurer(services, options);
        }

        private static Type FindApiOperationHandler(ApiOperationDescriptor apiOperationDescriptor, Type apiOperationHandlerType)
        {
            return apiOperationDescriptor.OperationType.Assembly.GetExportedTypes().SingleOrDefault(apiOperationHandlerType.IsAssignableFrom);
        }
    }
}
