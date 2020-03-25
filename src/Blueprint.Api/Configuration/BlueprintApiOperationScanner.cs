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
        // As scan or add operations are executed we add a number of actions that will be executed at a later
        // time to perform the actual scanning. This is done to allow the global conventions to be applied to
        // the scans, even if they are registered after the scan calls
        // i.e. scanOperations.Add(addType => addType(theTypeToRegisterFromMethodCall).
        private readonly List<Action<Action<Type>>> scanOperations = new List<Action<Action<Type>>>();

        private readonly List<IOperationScannerConvention> conventions = new List<IOperationScannerConvention>();

        /// <summary>
        /// Adds an <see cref="IOperationScannerConvention" /> that will be invoked for every
        /// <see cref="IApiOperation" /> that has been registered with this scanner to enable it to contribute and / or
        /// change details of the <see cref="ApiOperationDescriptor" />s, in addition to providing global filtering
        /// capabilities.
        /// </summary>
        /// <param name="contributor">The contributor.</param>
        /// <returns>This <see cref="BlueprintApiOperationScanner"/> for further configuration.</returns>
        public BlueprintApiOperationScanner AddConvention(IOperationScannerConvention contributor)
        {
            Guard.NotNull(nameof(contributor), contributor);

            conventions.Add(contributor);

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
            scanOperations.Add((add) =>
            {
                foreach (var assembly in assemblies)
                {
                    DoScan(assembly, filter, add);
                }
            });

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
            scanOperations.Add((add) =>
            {
                DoScan(assembly, filter, add);
            });

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

            scanOperations.Add(a => a(type));

            return this;
        }

        /// <summary>
        /// Given the operations that have been added registers them and all associated data with the given
        /// <see cref="ApiDataModel" />.
        /// </summary>
        /// <param name="dataModel">The data model to register the operations to.</param>
        public void Register(ApiDataModel dataModel)
        {
            foreach (var scanOperation in scanOperations)
            {
                scanOperation((t) => Register(dataModel, t));
            }
        }

        private void DoScan(Assembly assembly, Func<Type, bool> filter, Action<Type> add)
        {
            foreach (var type in GetExportedTypesOfInterface(assembly, typeof(IApiOperation)))
            {
                var canInclude = filter is null || filter(type);

                foreach (var globalFilters in conventions)
                {
                    if (!globalFilters.ShouldInclude(type))
                    {
                        canInclude = false;
                        break;
                    }
                }

                if (canInclude)
                {
                    add(type);
                }
            }
        }

        private void Register(ApiDataModel dataModel, Type type)
        {
            var apiOperationDescriptor = CreateApiOperationDescriptor(type);

            dataModel.RegisterOperation(apiOperationDescriptor);
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

            foreach (var c in conventions)
            {
                c.Apply(descriptor);
            }

            foreach (var linkAttribute in type.GetCustomAttributes<LinkAttribute>())
            {
                descriptor.AddLink(
                    new ApiOperationLink(descriptor, linkAttribute.Url, linkAttribute.Rel ?? descriptor.Name)
                    {
                        ResourceType = linkAttribute.ResourceType,
                    });
            }

            var typedOperation = type
                .GetInterfaces()
                .SingleOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IApiOperation<>));

            if (typedOperation != null)
            {
                descriptor.AddResponse(new ResponseDescriptor(typedOperation.GetGenericArguments()[0]));
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
