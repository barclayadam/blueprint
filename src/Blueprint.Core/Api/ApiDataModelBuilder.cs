﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

using Blueprint.Core.Authorisation;
using Blueprint.Core.Utilities;
using NHibernate.Util;

namespace Blueprint.Core.Api
{
    /// <summary>
    /// Serves as the central configuration point for the Blueprint API, providing the options for
    /// configuring behaviour as well as creating an <see cref="ApiDataModel" /> that represents the operations
    /// that can be executed.
    /// </summary>
    public class BlueprintApiOptions
    {
        private readonly ApiDataModel dataModel = new ApiDataModel();

        public BlueprintApiOptions(Action<BlueprintApiOptions> configure)
        {
            AddOperation<RootMetadataOperation>();

            configure(this);

            if (ApplicationName == null)
            {
                throw new InvalidOperationException("An application name MUST be set");
            }
        }

        /// <summary>
        /// Gets or sets what should happen if a request comes in and cannot be handled by any
        /// known handlers.
        /// </summary>
        public NotFoundMode NotFoundMode { get; set; } = NotFoundMode.Handle;

        /// <summary>
        /// Gets the list of middleware builder that will be used by the pipeline generation, added in the
        /// order that they will be configured.
        /// </summary>
        public List<Type> Middlewares { get; } = new List<Type>();

        /// <summary>
        /// Gets or sets the name of the application, which is used when generating the DLL for the pipeline
        /// executors.
        /// </summary>
        public string ApplicationName { get; set; }

        public ApiDataModel Model
        {
            get { return dataModel; }
        }

        /// <summary>
        /// Sets  the name of the application, which is used when generating the DLL for the pipeline
        /// executors.
        /// </summary>
        /// <param name="appName">The name of the application.</param>
        public void WithApplicationName(string appName)
        {
            Guard.NotNullOrEmpty(nameof(appName), appName);

            ApplicationName = appName;
        }

        public void UseMiddlewareBuilder<T>() where T : IMiddlewareBuilder
        {
            Middlewares.Add(typeof(T));
        }

        /// <summary>
        /// Scans the given assemblies for operations to register, with an optional filter function to exclude
        /// found types.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <param name="filter">The optional filter over found operations.</param>
        public void Scan(Assembly[] assemblies, Func<Type, bool> filter = null)
        {
            foreach (var a in assemblies)
            {
                Scan(a, filter);
            }
        }

        /// <summary>
        /// Scans the given assembly for operations to register, with an optional filter function to exclude
        /// found types.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="filter">The optional filter over found operations.</param>
        public void Scan(Assembly assembly, Func<Type, bool> filter = null)
        {
            filter = filter ?? ((t) => true);

            foreach (var t in GetExportedTypesOfInterface(assembly, typeof(IApiOperation)))
            {
                if (filter(t))
                {
                    AddOperation(t);
                }
            }
        }

        public void AddOperation<T>() where T : IApiOperation
        {
            AddOperation(typeof(T));
        }

        public void AddOperation(Type t)
        {
            if (!typeof(IApiOperation).IsAssignableFrom(t))
            {
                throw new ArgumentException($"Type {t.FullName} does not implement {nameof(IApiOperation)}, cannot register.");
            }

            var o = CreateApiOperationDescriptor(t);

            dataModel.RegisterOperation(o);

            o.OperationType
                .GetCustomAttributes<LinkAttribute>()
                .ForEach(l => dataModel.RegisterLink(new ApiOperationLink(o, l.Url, l.Rel ?? o.Name)
                {
                    ResourceType = l.ResourceType
                }));
        }

        private static ApiOperationDescriptor CreateApiOperationDescriptor(Type t)
        {
            HttpMethod supportedMethod;

            var httpMethodAttribute = t.GetCustomAttribute<HttpMethodAttribute>(true);

            if (httpMethodAttribute != null)
            {
                supportedMethod = new HttpMethod(httpMethodAttribute.HttpMethod);
            }
            else
            {
                // By default, command are POST and everything else GET
                supportedMethod = typeof(ICommand).IsAssignableFrom(t) ? HttpMethod.Post : HttpMethod.Get;
            }

            return new ApiOperationDescriptor(t, supportedMethod)
            {
                AnonymousAccessAllowed = t.HasAttribute<AllowAnonymousAttribute>(true),
                IsExposed = t.HasAttribute<UnexposedOperationAttribute>(true) == false,
                ShouldAudit = !t.HasAttribute<DoNotAuditOperationAttribute>(true),
                RecordPerformanceMetrics = !t.HasAttribute<DoNotRecordPerformanceMetricsAttribute>(true)
            };
        }

        private static IEnumerable<Type> GetExportedTypesOfInterface(Assembly assembly, Type interfaceType)
        {
            var allExportedTypes = assembly.GetExportedTypes();

            return allExportedTypes.Where(t => !t.IsAbstract && t.GetInterface(interfaceType.Name) != null);
        }
    }
}
