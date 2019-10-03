﻿using System;
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
using Blueprint.Core.Caching;
using Blueprint.Core.Errors;
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

            services.AddSingleton(options);
            services.AddSingleton(options.Model);
            services.AddSingleton<ApiConfiguration>();
            services.AddSingleton<ApiLinkGenerator>();
            services.AddSingleton<AssemblyGenerator>();
            services.AddSingleton<ToFileCompileStrategy>();
            services.AddSingleton<InMemoryOnlyCompileStrategy>();
            services.AddSingleton<AssemblyGenerator>();
            services.AddSingleton<IApiOperationExecutor>(s => new ApiOperationExecutorBuilder(s.GetRequiredService<ILogger<ApiOperationExecutorBuilder>>()).Build(options, s));

            services.AddScoped<IErrorLogger, ErrorLogger>();

            services.AddScoped<IApiAuthoriserAggregator, ApiAuthoriserAggregator>();

            // Tasks: services.AddScoped<IBackgroundTaskScheduler, BackgroundTaskScheduler>();
            // Tasks: Decorate: services.AddScoped<IBackgroundTaskScheduleProvider, ActivityTrackingBackgroundTaskScheduleProvider<Hangfire>>();

            services.AddSingleton<ITypeFormatter, JsonTypeFormatter>();
            services.AddScoped<IResourceLinkGenerator, EntityOperationResourceLinkGenerator>();
            services.AddScoped<IApiAuthoriser, MustBeAuthenticatedApiAuthoriser>();

            // Validation
            services.AddSingleton<IValidationSource, DataAnnotationsValidationSource>();
            services.AddSingleton<IValidationSource, BlueprintValidationSource>();
            services.AddSingleton<IValidationSourceBuilder, DataAnnotationsValidationSourceBuilder>();
            services.AddSingleton<IValidationSourceBuilder, BlueprintValidationSourceBuilder>();
            services.AddSingleton<IValidator, BlueprintValidator>();

            services.AddSingleton<ICache, Cache>();
            services.AddSingleton(MemoryCache.Default);
            services.AddSingleton<IExceptionFilter, BasicExceptionFilter>();

            services.AddTransient<IInstanceFrameProvider, DefaultInstanceFrameProvider>();

            // Formatters
            services.AddSingleton<JsonTypeFormatter>();

            // Random infrastructure
            services.AddSingleton<IHttpRequestStreamReaderFactory, MemoryPoolHttpRequestStreamReaderFactory>();
            services.AddSingleton<IHttpResponseStreamWriterFactory, MemoryPoolHttpResponseStreamWriterFactory>();

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
