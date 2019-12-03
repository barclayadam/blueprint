using System;
using System.Linq;
using Blueprint.Api.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Model;
using Blueprint.Core.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api
{
    public class DefaultInstanceFrameProvider : IInstanceFrameProvider
    {
        private readonly IServiceProvider provider;

        public DefaultInstanceFrameProvider(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public GetInstanceFrame<T> VariableFromContainer<T>(GeneratedType generatedType, Type toLoad)
        {
            var registrations = (IServiceCollection)provider.GetService(typeof(IServiceCollection));
            var registrationsForType = registrations.Where(r => r.ServiceType == toLoad).ToList();

            if (registrationsForType.Any() == false)
            {
                // If we are trying to grab a list of services then having none registered is OK, as we assume
                // the user of this service expects 0 or more
                if (!toLoad.IsEnumerable())
                {
                    throw new InvalidOperationException(
                        $"No registrations exist for the service type {toLoad.FullName}. If you are using the default IoC container that is " +
                        "built-in (Microsoft.Extensions.DependencyInjection) then you MUST register all services up-front, including concrete classes. If you " +
                        "are using an IoC container that does allow creating unregistered types (i.e. StructureMap) make sure you have registered that within " +
                        "your Blueprint setup.");
                }
            }

            if (registrationsForType.Count == 1)
            {
                // When there is only one possible type that could be created from the IoC container
                // we can do a little more optimisation.
                var instanceRef = registrationsForType.Single();

                if (instanceRef.Lifetime == ServiceLifetime.Singleton)
                {
                    // We have a singleton object, which means we can have this injected at build time of the
                    // pipeline executor which will only be constructed once.
                    var injected = new InjectedField(toLoad);

                    foreach (var alreadyInjected in generatedType.AllInjectedFields)
                    {
                        // We already have injected an instance of this interface as a concrete class, leading to a duplicate
                        if (injected.ArgumentName == alreadyInjected.ArgumentName)
                        {
                            throw new InvalidOperationException(
                                "An attempt has been made to request a service form the DI container that will lead " +
                                "to a duplicate constructor argument. This happens when a service is requested by an interface AND " +
                                $"it's concrete type when they differ only be the I prefix (i.e. {toLoad.FullName} vs {alreadyInjected.VariableType.FullName}).\n\n" +
                                $"To fix this ensure that this service is only referenced by it's interface type of {toLoad.FullName}");
                        }
                    }

                    generatedType.AllInjectedFields.Add(injected);

                    return new InjectedFrame<T>(injected);
                }
            }

            return new TransientInstanceFrame<T>(toLoad);
        }
    }
}
