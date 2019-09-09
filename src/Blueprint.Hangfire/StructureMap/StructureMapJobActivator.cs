using System;

using global::StructureMap;

using Hangfire;

namespace Blueprint.Hangfire.StructureMap
{
    /// <summary>
    /// StructureMap Job Activator.
    /// </summary>
    public class StructureMapJobActivator : JobActivator
    {
        private readonly IContainer rootContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureMapJobActivator"/>
        /// class with a given StructureMap container
        /// </summary>
        /// <param name="container">Container that will be used to create instances of classes during
        /// the job activation process</param>
        public StructureMapJobActivator(IContainer container)
        {
            this.rootContainer = container ?? throw new ArgumentNullException(nameof(container));
        }

        /// <inheritdoc />
        public override object ActivateJob(Type jobType)
        {
            return rootContainer.GetInstance(jobType);
        }

        /// <inheritdoc />
        public override JobActivatorScope BeginScope()
        {
            return new StructureMapDependencyScope(rootContainer.GetNestedContainer());
        }

        private class StructureMapDependencyScope : JobActivatorScope
        {
            private readonly IContainer nestedContainer;

            public StructureMapDependencyScope(IContainer nestedContainer)
            {
                this.nestedContainer = nestedContainer;
            }

            public override object Resolve(Type type)
            {
                return nestedContainer.GetInstance(type);
            }

            public override void DisposeScope()
            {
                nestedContainer.Dispose();
            }
        }
    }
}
