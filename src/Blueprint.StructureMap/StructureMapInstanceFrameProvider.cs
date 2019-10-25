using System;
using System.Linq;
using Blueprint.Api;
using Blueprint.Api.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Model;
using StructureMap;
using StructureMap.Pipeline;

namespace Blueprint.StructureMap
{
    public class StructureMapInstanceFrameProvider : IInstanceFrameProvider
    {
        private readonly IContainer container;

        public StructureMapInstanceFrameProvider(IContainer container)
        {
            this.container = container;
        }

        public GetInstanceFrame<T> VariableFromContainer<T>(GeneratedType generatedType, Type toLoad)
        {
            var config = container.Model.For(toLoad);

            if (config.HasImplementations() && config.Instances.Count() == 1)
            {
                // When there is only one possible type that could be created from the IoC container
                // we can do a little more optimisation.
                var instanceRef = config.Instances.Single();

                if (instanceRef.Lifecycle is SingletonLifecycle)
                {
                    // We have a singleton object, which means we can have this injected at build time of the
                    // pipeline executor which will only be constructed once.
                    var injected = new InjectedField(toLoad);

                    generatedType.AllInjectedFields.Add(injected);

                    return new InjectedFrame<T>(injected);
                }

                if (instanceRef.Instance is IConfiguredInstance)
                {
                    // Small tweak to resolve the actual known type. Makes generated code a little nicer as it
                    // makes it obvious what is _actually_ going to be built without knowledge of the container
                    // setup. This is possible because SM knows how to build concrete types even if not pre-registered
                    //
                    // Note we cannot do this for all types, for example a lambda as that means extra configuration that
                    // would not be registered for the concrete type.
                    // i.e. For<IDatabaseConnectionFactory().Use(() => new SqlServerConnectionFactory("my connection string"))
                    // would fail if you grab SqlServerConnectionFactory directly as it has not been configured, it's only been
                    // configured to use "my connection string" when getting instance of IDatabaseConnectionFactory.
                    return new TransientInstanceFrame<T>(toLoad, instanceRef.ReturnedType);
                }
            }

            return new TransientInstanceFrame<T>(toLoad);
        }
    }
}
