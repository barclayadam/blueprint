using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api
{
    /// <summary>
    /// Implements an <see cref="InstanceFrameProvider" /> for Microsoft's / .NETs DI container (<see cref="IServiceProvider" />).
    /// </summary>
    [UsedImplicitly]
    public class MicrosoftDependencyInjectionInstanceFrameProvider : InstanceFrameProvider
    {
        private readonly IServiceProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftDependencyInjectionInstanceFrameProvider" /> class.
        /// </summary>
        /// <param name="provider">The service provider from which instances would be grabbed at runtime.</param>
        public MicrosoftDependencyInjectionInstanceFrameProvider(IServiceProvider provider)
        {
            this.provider = provider;
        }

        /// <inheritdoc />
        protected override string GetNoRegistrationExistsExceptionMessage(Type toLoad)
        {
            return $"No registrations exist for the service type {toLoad.FullName} in the registered " +
                   "IoC container (Microsoft.Extensions.DependencyInjection). You MUST register all services up-front in the ServiceCollection " +
                   "used to build the IServiceProvider for Blueprint. If you are using a third-party container then it needs to be " +
                   "registered with Blueprint (i.e. BlueprintApiBuilder.AddStructureMap)";
        }

        /// <inheritdoc />
        protected override IEnumerable<IoCRegistration> GetRegistrations(Type type)
        {
            // NB: We rely on the service collection being registered when configuring Blueprint
            var registrations = provider.GetRequiredService<IServiceCollection>();
            var registrationsForType = new List<IoCRegistration>();

            foreach (var r in registrations)
            {
                // First, obvious check for exact type matching
                var isMatch = r.ServiceType == type;

                // Handle small subset of open-generic registrations. This can be made more robust, but
                // is, for now, very basic to handle in particular the common IOptions registration pattern
                //
                // - if service type == interface [TypeX]<> and type is interface of [TypeX]<T> then match
                //   (i.e. IOptions<any> registration would match IOptions<MyOptions> type
                if (r.ServiceType.IsGenericTypeDefinition && r.ServiceType.IsInterface)
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == r.ServiceType)
                    {
                        isMatch = true;
                    }
                }

                if (isMatch)
                {
                    registrationsForType.Add(new IoCRegistration
                    {
                        ServiceType = r.ServiceType,
                        IsSingleton = r.Lifetime == ServiceLifetime.Singleton,
                    });
                }
            }

            return registrationsForType;
        }
    }
}
