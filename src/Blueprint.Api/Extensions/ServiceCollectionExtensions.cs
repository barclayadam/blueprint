using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Blueprint.Api;
using Blueprint.Api.Authorisation;
using Blueprint.Api.Errors;
using Blueprint.Api.Formatters;
using Blueprint.Api.Validation;
using Blueprint.Core.Caching;
using Blueprint.Core.Errors;

// This is the recommendation from MS for extensions to IServiceCollection to aid discoverability
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static BlueprintConfigurer AddBlueprintApi(this IServiceCollection services, BlueprintApiOptions options)
        {
            services.AddSingleton(options);
            services.AddSingleton(options.Model);
            services.AddSingleton<IApiOperationExecutor>(s => new ApiOperationExecutorBuilder().Build(options, s));

            services.AddScoped<IErrorLogger, ErrorLogger>();

            services.AddScoped<IApiAuthoriserAggregator, ApiAuthoriserAggregator>();

            // Tasks: services.AddScoped<IBackgroundTaskScheduler, BackgroundTaskScheduler>();
            // Tasks: Decorate: services.AddScoped<IBackgroundTaskScheduleProvider, ActivityTrackingBackgroundTaskScheduleProvider<Hangfire>>();

            services.AddScoped<ITypeFormatter, JsonTypeFormatter>();
            services.AddScoped<IResourceLinkGenerator, EntityOperationResourceLinkGenerator>();
            services.AddScoped<IApiAuthoriser, MustBeAuthenticatedApiAuthoriser>();

            // Validation
            services.AddScoped<IValidationSource, DataAnnotationsValidationSource>();
            services.AddScoped<IValidationSource, BlueprintValidationSource>();
            services.AddScoped<IValidationSourceBuilder, DataAnnotationsValidationSourceBuilder>();
            services.AddScoped<IValidationSourceBuilder, BlueprintValidationSourceBuilder>();
            services.AddSingleton<IValidator, BlueprintValidator>();

            services.AddSingleton<ICache, Cache>();
            services.AddSingleton(MemoryCache.Default);
            services.AddSingleton<IExceptionFilter, BasicExceptionFilter>();

            services.AddTransient<IInstanceFrameProvider, DefaultInstanceFrameProvider>();

            var missingApiOperationHandlers = new List<ApiOperationDescriptor>();

            foreach (var operation in options.Model.Operations)
            {
                var apiOperationHandlerType = typeof(IApiOperationHandler<>).MakeGenericType(operation.OperationType);
                var apiOperationHandler = FindApiOperationHandler(operation, apiOperationHandlerType);

                if (apiOperationHandler == null)
                {
                    missingApiOperationHandlers.Add(operation);
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
