using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Blueprint.Api.Authorisation;
using Blueprint.Compiler;
using Blueprint.Core;
using Blueprint.Core.Authorisation;
using Blueprint.Core.Utilities;
using Microsoft.CodeAnalysis;

namespace Blueprint.Api.Configuration
{
    /// <summary>
    /// Serves as the central configuration point for the Blueprint API, providing the options for
    /// configuring behaviour as well as creating an <see cref="ApiDataModel" /> that represents the operations
    /// that can be executed.
    /// </summary>
    public class BlueprintApiOptions
    {
        private readonly ApiDataModel dataModel;

        public BlueprintApiOptions(Action<BlueprintApiOptions> configure = null)
        {
            Rules = new GenerationRules("Blueprint.Pipelines")
            {
                OptimizationLevel = OptimizationLevel.Release,
            };

            dataModel = new ApiDataModel();

            AddOperation<RootMetadataOperation>();
            configure?.Invoke(this);
        }

        public GenerationRules Rules { get; }

        /// <summary>
        /// Gets the list of middleware builder that will be used by the pipeline generation, added in the
        /// order that they will be configured.
        /// </summary>
        public List<Type> Middlewares { get; } = new List<Type>();

        public ApiDataModel Model => dataModel;

        public string ApplicationName { get; private set; }

        /// <summary>
        /// Sets  the name of the application, which is used when generating the DLL for the pipeline
        /// executors.
        /// </summary>
        /// <param name="applicationName">The name of the application.</param>
        public void WithApplicationName(string applicationName)
        {
            Guard.NotNullOrEmpty(nameof(applicationName), applicationName);

            ApplicationName = applicationName;
        }

        public void UseMiddlewareBuilder<T>() where T : IMiddlewareBuilder
        {
            Middlewares.Add(typeof(T));
        }

        public void UseMiddlewareBuilder(Type middlewareType)
        {
            if (!typeof(IMiddlewareBuilder).IsAssignableFrom(middlewareType))
            {
                throw new ArgumentException(
                    $"Builder type must implement {nameof(IMiddlewareBuilder)}, but {middlewareType} does not.",
                    nameof(middlewareType));
            }

            Middlewares.Add(middlewareType);
        }

        /// <summary>
        /// Scans the given assemblies for operations to register, with an optional filter function to exclude
        /// found types.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <param name="filter">The optional filter over found operations.</param>
        public void ScanForOperations(Assembly[] assemblies, Func<Type, bool> filter = null)
        {
            foreach (var assembly in assemblies)
            {
                ScanForOperations(assembly, filter);
            }
        }

        /// <summary>
        /// Scans the given assembly for operations to register, with an optional filter function to exclude
        /// found types.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="filter">The optional filter over found operations.</param>
        public void ScanForOperations(Assembly assembly, Func<Type, bool> filter = null)
        {
            filter = filter ?? (t => true);

            foreach (var type in GetExportedTypesOfInterface(assembly, typeof(IApiOperation)))
            {
                if (filter(type))
                {
                    AddOperation(type);
                }
            }
        }

        public void AddOperation<T>() where T : IApiOperation
        {
            AddOperation(typeof(T));
        }

        public void AddOperation(Type type)
        {
            if (!typeof(IApiOperation).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Type {type.FullName} does not implement {nameof(IApiOperation)}, cannot register.");
            }

            var apiOperationDescriptor = CreateApiOperationDescriptor(type);

            dataModel.RegisterOperation(apiOperationDescriptor);

            foreach (var linkAttribute in apiOperationDescriptor.OperationType.GetCustomAttributes<LinkAttribute>())
            {
                dataModel.RegisterLink(new ApiOperationLink(apiOperationDescriptor, linkAttribute.Url, linkAttribute.Rel ?? apiOperationDescriptor.Name) {ResourceType = linkAttribute.ResourceType});
            }
        }

        private static ApiOperationDescriptor CreateApiOperationDescriptor(Type type)
        {
            HttpMethod supportedMethod;

            var httpMethodAttribute = type.GetCustomAttribute<HttpMethodAttribute>(true);

            if (httpMethodAttribute != null)
            {
                supportedMethod = new HttpMethod(httpMethodAttribute.HttpMethod);
            }
            else
            {
                // By default, command are POST and everything else GET
                supportedMethod = typeof(ICommand).IsAssignableFrom(type) ? HttpMethod.Post : HttpMethod.Get;
            }

            return new ApiOperationDescriptor(type, supportedMethod)
            {
                AnonymousAccessAllowed = type.HasAttribute<AllowAnonymousAttribute>(true),
                IsExposed = type.HasAttribute<UnexposedOperationAttribute>(true) == false,
                ShouldAudit = !type.HasAttribute<DoNotAuditOperationAttribute>(true),
                RecordPerformanceMetrics = !type.HasAttribute<DoNotRecordPerformanceMetricsAttribute>(true),
            };
        }

        private static IEnumerable<Type> GetExportedTypesOfInterface(Assembly assembly, Type interfaceType)
        {
            var allExportedTypes = assembly.GetExportedTypes();

            return allExportedTypes.Where(t => !t.IsAbstract && t.GetInterface(interfaceType.Name) != null);
        }
    }
}
