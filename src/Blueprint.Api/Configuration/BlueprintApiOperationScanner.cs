using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blueprint.Api.Authorisation;
using Blueprint.Core;
using Blueprint.Core.Utilities;

namespace Blueprint.Api.Configuration
{
    /// <summary>
    /// Provides the scanning and creation of <see cref="ApiOperationDescriptor" />s and associated
    /// <see cref="ApiResource" />s and <see cref="Link" />s.
    /// </summary>
    public class BlueprintApiOperationScanner
    {
        private readonly List<IOperationScannerFeatureContributor> contributors = new List<IOperationScannerFeatureContributor>();
        private readonly List<Type> operationsToRegister = new List<Type>();

        /// <summary>
        /// Initialises a new instance of the <see cref="BlueprintApiOperationScanner" /> class with a single
        /// <see cref="RootMetadataOperation" /> automatically added.
        /// </summary>
        public BlueprintApiOperationScanner()
        {
            AddOperation<RootMetadataOperation>();
        }

        /// <summary>
        /// Adds a <see cref="IOperationScannerFeatureContributor" /> that will be invoked for every
        /// <see cref="IApiOperation" /> that has been registered with this scanner to enable it to contribute and / or
        /// change details of the <see cref="ApiOperationDescriptor" />s.
        /// </summary>
        /// <param name="contributor">The contributor.</param>
        /// <returns>This <see cref="BlueprintApiOperationScanner"/> for further configuration.</returns>
        public BlueprintApiOperationScanner AddContributor(IOperationScannerFeatureContributor contributor)
        {
            Guard.NotNull(nameof(contributor), contributor);

            contributors.Add(contributor);

            return this;
        }

        /// <summary>
        /// Scans the given assemblies for operations to register, with an optional filter function to exclude
        /// found types.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <param name="filter">The optional filter over found operations.</param>
        /// <returns>This <see cref="BlueprintApiOperationScanner"/> for further configuration.</returns>
        public BlueprintApiOperationScanner ScanForOperations(Assembly[] assemblies, Func<Type, bool> filter = null)
        {
            foreach (var assembly in assemblies)
            {
                ScanForOperations(assembly, filter);
            }

            return this;
        }

        /// <summary>
        /// Scans the given assembly for operations to register, with an optional filter function to exclude
        /// found types.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="filter">The optional filter over found operations.</param>
        /// <returns>This <see cref="BlueprintApiOperationScanner"/> for further configuration.</returns>
        public BlueprintApiOperationScanner ScanForOperations(Assembly assembly, Func<Type, bool> filter = null)
        {
            foreach (var type in GetExportedTypesOfInterface(assembly, typeof(IApiOperation)))
            {
                if (filter is null || filter(type))
                {
                    AddOperation(type);
                }
            }

            return this;
        }

        /// <summary>
        /// Adds the operation identified by <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IApiOperation"/> to register.</typeparam>
        /// <returns>This <see cref="BlueprintApiOperationScanner"/> for further configuration.</returns>
        public BlueprintApiOperationScanner AddOperation<T>() where T : IApiOperation
        {
            AddOperation(typeof(T));

            return this;
        }

        /// <summary>
        /// Adds the operation identified by <paramref name="type" />.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> representing the <see cref="IApiOperation"/> to register.</param>
        /// <returns>This <see cref="BlueprintApiOperationScanner"/> for further configuration.</returns>
        public BlueprintApiOperationScanner AddOperation(Type type)
        {
            if (!typeof(IApiOperation).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Type {type.FullName} does not implement {nameof(IApiOperation)}, cannot register.");
            }

            operationsToRegister.Add(type);

            return this;
        }

        /// <summary>
        /// Bulk registers all operations in the given enumeration.
        /// </summary>
        /// <param name="types">The types to register.</param>
        /// <returns>This <see cref="BlueprintApiOperationScanner"/> for further configuration.</returns>
        /// <see cref="AddOperation" />
        public BlueprintApiOperationScanner AddOperations(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                AddOperation(type);
            }

            return this;
        }

        /// <summary>
        /// Given the operations that have been added registers them and all associated data with the given
        /// <see cref="ApiDataModel" />.
        /// </summary>
        /// <param name="dataModel">The data model to register the operations to.</param>
        public void Register(ApiDataModel dataModel)
        {
            foreach (var type in operationsToRegister)
            {
                var apiOperationDescriptor = CreateApiOperationDescriptor(type);

                dataModel.RegisterOperation(apiOperationDescriptor);

                foreach (var linkAttribute in apiOperationDescriptor.OperationType.GetCustomAttributes<LinkAttribute>())
                {
                    dataModel.RegisterLink(
                        new ApiOperationLink(apiOperationDescriptor, linkAttribute.Url, linkAttribute.Rel ?? apiOperationDescriptor.Name)
                        {
                            ResourceType = linkAttribute.ResourceType,
                        });
                }
            }
        }

        private ApiOperationDescriptor CreateApiOperationDescriptor(Type type)
        {
            var descriptor = new ApiOperationDescriptor(type)
            {
                AnonymousAccessAllowed = type.HasAttribute<AllowAnonymousAttribute>(true),
                IsExposed = type.HasAttribute<UnexposedOperationAttribute>(true) == false,
                ShouldAudit = !type.HasAttribute<DoNotAuditOperationAttribute>(true),
                RecordPerformanceMetrics = !type.HasAttribute<DoNotRecordPerformanceMetricsAttribute>(true),
            };

            foreach (var c in contributors)
            {
                c.Apply(descriptor);
            }

            return descriptor;
        }

        private static IEnumerable<Type> GetExportedTypesOfInterface(Assembly assembly, Type interfaceType)
        {
            var allExportedTypes = assembly.GetExportedTypes();

            return allExportedTypes.Where(t => !t.IsAbstract && t.GetInterface(interfaceType.Name) != null);
        }
    }
}
