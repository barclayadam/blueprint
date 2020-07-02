using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using StructureMap;
using StructureMap.Pipeline;

namespace Blueprint.StructureMap
{
    /// <summary>
    /// Implements an <see cref="InstanceFrameProvider" /> for StructureMap (<see cref="IContainer" />).
    /// </summary>
    [UsedImplicitly]
    public class StructureMapInstanceFrameProvider : InstanceFrameProvider
    {
        private readonly IContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureMapInstanceFrameProvider" /> class.
        /// </summary>
        /// <param name="container">The container from which instances would be grabbed at runtime.</param>
        public StructureMapInstanceFrameProvider(IContainer container)
        {
            this.container = container;
        }

        /// <inheritdoc />
        protected override string GetNoRegistrationExistsExceptionMessage(Type toLoad)
        {
            return $"No registrations exist for the service type {toLoad.FullName} in the registered IoC container (StructureMap).";
        }

        /// <inheritdoc />
        protected override IEnumerable<IoCRegistration> GetRegistrations(Type type)
        {
            var instanceRefs = container.Model.AllInstances.Where(i => i.ReturnedType == type || i.PluginType == type).ToArray();

            // IF the instanceRefs are empty AND type is a concrete rather than an interface then return a single instance
            // IsAbstract is true for interfaces
            if (instanceRefs.Length == 0 && !type.IsAbstract)
            {
                return new[] { new IoCRegistration { IsSingleton = false, ServiceType = type } };
            }

            return instanceRefs.Select(i => new IoCRegistration
            {
                ServiceType = i.PluginType,

                // We do not want to mark this as singleton, and therefore have it injected unless it's been explicitly registered
                // because otherwise when we try to build the executors ActivatorUtilities is used and the StructureMap container will NOT
                // return an "optional" registration from TryGetInstance (see https://structuremap.github.io/resolving/try-getting-an-optional-service-by-plugin-type/)
                //
                // Therefore we only mark as singleton if the PluginType is exactly the type requested, as we know in that case that
                // it _will_ resolve directly from the container
                IsSingleton = i.PluginType == type && i.Lifecycle is SingletonLifecycle,
            });
        }
    }
}
