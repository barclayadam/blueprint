using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Blueprint.Authorisation;
using Blueprint.Utilities;

namespace Blueprint.Configuration
{
    /// <summary>
    /// Provides the scanning and creation of <see cref="ApiOperationDescriptor" />s and associated
    /// <see cref="ApiResource" />s and <see cref="Link" />s.
    /// </summary>
    public class OperationScanner
    {
        // As scan or add operations are executed we add a number of actions that will be executed at a later
        // time to perform the actual scanning. This is done to allow the global conventions to be applied to
        // the scans, even if they are registered after the scan calls
        // i.e. scanOperations.Add(addType => addType(theTypeToRegisterFromMethodCall).
        private readonly List<Action<RegisterOperation>> _scanOperations = new List<Action<RegisterOperation>>();

        private readonly List<IOperationScannerConvention> _conventions = new List<IOperationScannerConvention>();
        private readonly List<Assembly> _scannedAssemblies = new List<Assembly>();

        /// <summary>
        /// Initialises a new instance of the <see cref="OperationScanner" /> class.
        /// </summary>
        public OperationScanner()
        {
            this._conventions.Add(new XmlDocResponseConvention());
            this._conventions.Add(new ApiExceptionFactoryResponseConvention());
            this._conventions.Add(new CommandOrQueryIsSupportedConvention());
        }

        private delegate void RegisterOperation(Type operationType, string source, Action<ApiOperationDescriptor> configure);

        /// <summary>
        /// The assemblies that have been registered to be scanned for operations.
        /// </summary>
        public IReadOnlyList<Assembly> ScannedAssemblies => this._scannedAssemblies;

        /// <summary>
        /// Adds an <see cref="IOperationScannerConvention" /> that will be invoked for every
        /// operation that has been registered with this scanner to enable it to contribute and / or
        /// change details of the <see cref="ApiOperationDescriptor" />s, in addition to providing global filtering
        /// capabilities.
        /// </summary>
        /// <param name="contributor">The contributor.</param>
        /// <returns>This <see cref="OperationScanner"/> for further configuration.</returns>
        public OperationScanner AddConvention(IOperationScannerConvention contributor)
        {
            Guard.NotNull(nameof(contributor), contributor);

            this._conventions.Add(contributor);

            return this;
        }

        /// <summary>
        /// Scans the calling assembly (<see cref="Assembly.GetCallingAssembly"/> for operations and handlers to
        /// register, with an optional filter function to exclude found types.
        /// </summary>
        /// <param name="filter">The optional filter over found operations.</param>
        /// <param name="configure">An optional action that can modify the created <see cref="ApiOperationDescriptor" />.</param>
        /// <returns>This <see cref="OperationScanner"/> for further configuration.</returns>
        public OperationScanner ScanCallingAssembly(Func<Type, bool> filter = null, Action<ApiOperationDescriptor> configure = null)
        {
            return this.Scan(Assembly.GetCallingAssembly(), filter, configure);
        }

        /// <summary>
        /// Scans the assembly that the <typeparamref name="T" /> type is contained in, plus
        /// any recursive references that assembly has that match the given filter.
        /// </summary>
        /// <remarks>
        /// Although it is possible to pass an assembly filter that always returns <c>true</c> it
        /// is highly recommended that this is <b>NOT</b> done as that can leave to a performance
        /// penalty on startup as thousands of types may need to be checked.
        /// </remarks>
        /// <param name="assemblyFilter">A filter to determine whether to scan a given assembly.</param>
        /// <param name="filter">A type filter to exclude types from the search.</param>
        /// <param name="configure">An optional action that can modify the created <see cref="ApiOperationDescriptor" />.</param>
        /// <typeparam name="T">The type from which an <see cref="Assembly"/> is loaded and recursively
        /// scanned.</typeparam>
        /// <returns>This <see cref="OperationScanner"/> for further configuration.</returns>
        public OperationScanner ScanReferencedAssembliesOf<T>(
            Func<AssemblyName, bool> assemblyFilter,
            Func<Type, bool> filter = null,
            Action<ApiOperationDescriptor> configure = null)
        {
            void ScanRecursive(Assembly a, ISet<Assembly> seen)
            {
                if (seen.Contains(a))
                {
                    return;
                }

                seen.Add(a);

                if (assemblyFilter(a.GetName()))
                {
                    this.Scan(a, filter, configure);
                }

                foreach (var referenced in a.GetReferencedAssemblies())
                {
                    ScanRecursive(AssemblyLoadContext.Default.LoadFromAssemblyName(referenced), seen);
                }
            }

            ScanRecursive(typeof(T).Assembly, new HashSet<Assembly>());

            return this;
        }

        /// <summary>
        /// Scans the given assemblies for operations and handlers to register, with an optional filter function to exclude
        /// found types.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <param name="filter">The optional filter over found operations.</param>
        /// <param name="configure">An optional action that can modify the created <see cref="ApiOperationDescriptor" />.</param>
        /// <returns>This <see cref="OperationScanner"/> for further configuration.</returns>
        public OperationScanner Scan(Assembly[] assemblies, Func<Type, bool> filter = null, Action<ApiOperationDescriptor> configure = null)
        {
            foreach (var assembly in assemblies)
            {
                this.Scan(assembly, filter, configure);
            }

            return this;
        }

        /// <summary>
        /// Scans the given assembly for operations and handlers to register, with an optional filter function to exclude
        /// found types.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="filter">The optional filter over found operations.</param>
        /// <param name="configure">An optional action that can modify the created <see cref="ApiOperationDescriptor" />.</param>
        /// <returns>This <see cref="OperationScanner"/> for further configuration.</returns>
        public OperationScanner Scan(Assembly assembly, Func<Type, bool> filter = null, Action<ApiOperationDescriptor> configure = null)
        {
            if (this.ScannedAssemblies.Contains(assembly))
            {
                return this;
            }

            this._scannedAssemblies.Add(assembly);

            this._scanOperations.Add((add) =>
            {
                this.DoScan(assembly, filter, add, configure);
            });

            return this;
        }

        /// <summary>
        /// Bulk registers all operations in the given enumeration.
        /// </summary>
        /// <param name="types">The types to register.</param>
        /// <param name="source">The source of these operations, useful for tracking <em>where</em> an operation comes from. Used for
        /// diagnostics.</param>
        /// <param name="configure">An optional action that can modify the created <see cref="ApiOperationDescriptor" />.</param>
        /// <returns>This <see cref="OperationScanner"/> for further configuration.</returns>
        /// <see cref="AddOperation" />
        public OperationScanner AddOperations(IEnumerable<Type> types, string source = "AddOperations(Type[])", Action<ApiOperationDescriptor> configure = null)
        {
            foreach (var type in types)
            {
                this.AddOperation(type, source, configure);
            }

            return this;
        }

        /// <summary>
        /// Adds the operation identified by <typeparamref name="T"/>.
        /// </summary>
        /// <param name="source">The source of this operation, useful for tracking <em>where</em> an operation comes from. Used for
        /// diagnostics.</param>
        /// <param name="configure">An optional action that can modify the created <see cref="ApiOperationDescriptor" />.</param>
        /// <typeparam name="T">The type of operation to register.</typeparam>
        /// <returns>This <see cref="OperationScanner"/> for further configuration.</returns>
        public OperationScanner AddOperation<T>(string source = null, Action<ApiOperationDescriptor> configure = null)
        {
            this.AddOperation(typeof(T), source ?? $"AddOperation<{typeof(T).Name}>()", configure);

            return this;
        }

        /// <summary>
        /// Adds the operation identified by <paramref name="type" />.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> representing the operation to register.</param>
        /// <param name="source">The source of this operation, useful for tracking <em>where</em> an operation comes from. Used for
        /// diagnostics.</param>
        /// <param name="configure">An optional action that can modify the created <see cref="ApiOperationDescriptor" />.</param>
        /// <returns>This <see cref="OperationScanner"/> for further configuration.</returns>
        public OperationScanner AddOperation(Type type, string source = null, Action<ApiOperationDescriptor> configure = null)
        {
            this._scanOperations.Add(a => a(type, source ?? $"AddOperation(typeof({type.Name}))", configure));

            return this;
        }

        /// <summary>
        /// Given the operations and scanners that have been added, finds all operations and
        /// registers them with the given <see cref="ApiDataModel" />.
        /// </summary>
        /// <param name="dataModel">The data model to register the operations to.</param>
        internal void FindOperations(ApiDataModel dataModel)
        {
            foreach (var scanOperation in this._scanOperations)
            {
                scanOperation((t, source, configure) => this.Register(dataModel, t, source, configure));
            }
        }

        private void DoScan(Assembly assembly, Func<Type, bool> filter, RegisterOperation register, Action<ApiOperationDescriptor> configure = null)
        {
            var source = $"Scan {assembly.FullName}";

            foreach (var type in assembly.GetExportedTypes())
            {
                // We only care about actual classes, and those that are NOT abstract.
                if (type.IsAbstract)
                {
                    continue;
                }

                // The filter passed in to an assembly scan can _remove_ any types that should
                // NOT be included
                if (filter?.Invoke(type) == false)
                {
                    continue;
                }

                // By default types are NOT included. It's only if a convention is positive
                // that a type will be included (note that only ONE convention needs to include the
                // message).
                foreach (var globalFilters in this._conventions)
                {
                    if (globalFilters.IsSupported(type))
                    {
                        register(type, source, configure);

                        break;
                    }
                }
            }
        }

        private void Register(ApiDataModel dataModel, Type type, string source, Action<ApiOperationDescriptor> configure)
        {
            var apiOperationDescriptor = this.CreateApiOperationDescriptor(type, source);

            configure?.Invoke(apiOperationDescriptor);

            dataModel.RegisterOperation(apiOperationDescriptor);
        }

        private ApiOperationDescriptor CreateApiOperationDescriptor(Type type, string source)
        {
            var descriptor = new ApiOperationDescriptor(type, source)
            {
                AnonymousAccessAllowed = type.HasAttribute<AllowAnonymousAttribute>(true),
                IsExposed = type.HasAttribute<UnexposedOperationAttribute>(true) == false,
                ShouldAudit = !type.HasAttribute<DoNotAuditOperationAttribute>(true),
                RecordPerformanceMetrics = !type.HasAttribute<DoNotRecordPerformanceMetricsAttribute>(true),
            };

            foreach (var linkAttribute in type.GetCustomAttributes<LinkAttribute>())
            {
                descriptor.AddLink(
                    new ApiOperationLink(descriptor, linkAttribute.RoutePattern, linkAttribute.Rel ?? descriptor.Name, linkAttribute.ResourceType));
            }

            foreach (var c in this._conventions)
            {
                c.Apply(descriptor);
            }

            return descriptor;
        }
    }
}
