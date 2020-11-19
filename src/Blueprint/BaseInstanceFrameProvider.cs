using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Model;
using Blueprint.Utilities;

namespace Blueprint
{
    public abstract class InstanceFrameProvider
    {
        public GetInstanceFrame<T> GetVariableFromContainer<T>(GeneratedType generatedType, Type toLoad)
        {
            var variable = this.TryGetVariableFromContainer<T>(generatedType, toLoad);

            if (variable == null)
            {
                throw new InvalidOperationException(this.GetNoRegistrationExistsExceptionMessage(toLoad));
            }

            return variable;
        }

        public GetInstanceFrame<T> TryGetVariableFromContainer<T>(GeneratedType generatedType, Type toLoad)
        {
            // If the requested type is enumerable we will always return a transient frame as, even with nothing registered, we
            // want to fulfil this request as it's expected that a 0-filled enumerable is acceptable
            if (toLoad.IsEnumerable())
            {
                return new TransientInstanceFrame<T>(toLoad);
            }

            // NB: We rely on the service collection being registered when configuring Blueprint
            var registrationsForType = this.GetRegistrations(toLoad).ToList();

            if (registrationsForType.Any() == false)
            {
                return null;
            }

            if (registrationsForType.Count == 1)
            {
                // When there is only one possible type that could be created from the IoC container
                // we can do a little more optimisation.
                var instanceRef = registrationsForType.Single();

                if (instanceRef.IsSingleton)
                {
                    // We have a singleton object, which means we can have this injected at build time of the
                    // pipeline executor which will only be constructed once.
                    var injected = new InjectedField(toLoad);

                    foreach (var alreadyInjected in generatedType.AllInjectedFields)
                    {
                        // Bail early, we have found an injected field already that can be reused
                        if (injected.ArgumentName == alreadyInjected.ArgumentName && injected.VariableType == alreadyInjected.VariableType)
                        {
                            return new InjectedFrame<T>(alreadyInjected);
                        }

                        // We already have injected an instance of this interface as a concrete class, leading to a duplicate
                        if (injected.ArgumentName == alreadyInjected.ArgumentName && injected.VariableType != alreadyInjected.VariableType)
                        {
                            throw new InvalidOperationException(
                                $"An attempt has been made to request a service ({toLoad.FullName}) from the DI container that will lead " +
                                $"to a duplicate constructor argument (existing type is {alreadyInjected.VariableType.FullName}) . This can happen when a " +
                                "service is requested by an interface AND it's concrete type when they differ only be the I prefix. " +
                                "To fix this ensure that this service is only referenced by it's interface type");
                        }
                    }

                    generatedType.AllInjectedFields.Add(injected);

                    return new InjectedFrame<T>(injected);
                }
            }

            return new TransientInstanceFrame<T>(toLoad);
        }

        /// <summary>
        /// Gets the exception message that is thrown when no registration exists for the specified type, which may be
        /// different for each provider to give more concrete help to fix the issue.
        /// </summary>
        /// <param name="toLoad">The type that was to be loaded.</param>
        /// <returns>The exception message to use.</returns>
        protected abstract string GetNoRegistrationExistsExceptionMessage(Type toLoad);

        /// <summary>
        /// Gets all registrations in this provider for the specified type.
        /// </summary>
        /// <param name="type">The type to be loaded.</param>
        /// <returns>All registrations for the given type.</returns>
        protected abstract IEnumerable<IoCRegistration> GetRegistrations(Type type);

        /// <summary>
        /// A single registration in an IoC container for a type.
        /// </summary>
        protected class IoCRegistration
        {
            /// <summary>
            /// Gets or sets the service type of this registration (typically an interface).
            /// </summary>
            public Type ServiceType { get; set; }

            /// <summary>
            /// Whether the service is registered as a singleton (affects code generation).
            /// </summary>
            public bool IsSingleton { get; set; }
        }
    }
}
