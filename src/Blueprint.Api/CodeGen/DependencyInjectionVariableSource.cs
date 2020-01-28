using System;
using Blueprint.Compiler;
using Blueprint.Compiler.Model;

namespace Blueprint.Api.CodeGen
{
    /// <summary>
    /// An <see cref="IVariableSource"/> that will create variables of any type by getting the variable from the container
    /// that is used in an operation.
    /// </summary>
    public class DependencyInjectionVariableSource : IVariableSource
    {
        private readonly GeneratedMethod generatedMethod;
        private readonly InstanceFrameProvider instanceFrameProvider;

        /// <summary>
        /// Constructs a new instance of the <see cref="DependencyInjectionVariableSource" /> class.
        /// </summary>
        /// <param name="generatedMethod">The method this source is for.</param>
        /// <param name="instanceFrameProvider">The instance frame provider that does the work of creating an appropriate frame.</param>
        public DependencyInjectionVariableSource(GeneratedMethod generatedMethod, InstanceFrameProvider instanceFrameProvider)
        {
            this.generatedMethod = generatedMethod;
            this.instanceFrameProvider = instanceFrameProvider;
        }

        /// <inheritdoc />
        public Variable TryFindVariable(IMethodVariables variables, Type type)
        {
            return instanceFrameProvider
                .TryGetVariableFromContainer<object>(generatedMethod.GeneratedType, type)
                ?.InstanceVariable;
        }
    }
}
