using System;
using Blueprint.Compiler;
using Blueprint.Compiler.Model;

namespace Blueprint.CodeGen
{
    /// <summary>
    /// An <see cref="IVariableSource"/> that will create variables of any type by getting the variable from the container
    /// that is used in an operation.
    /// </summary>
    public class DependencyInjectionVariableSource : IVariableSource
    {
        private readonly GeneratedMethod _generatedMethod;
        private readonly InstanceFrameProvider _instanceFrameProvider;

        /// <summary>
        /// Constructs a new instance of the <see cref="DependencyInjectionVariableSource" /> class.
        /// </summary>
        /// <param name="generatedMethod">The method this source is for.</param>
        /// <param name="instanceFrameProvider">The instance frame provider that does the work of creating an appropriate frame.</param>
        public DependencyInjectionVariableSource(GeneratedMethod generatedMethod, InstanceFrameProvider instanceFrameProvider)
        {
            this._generatedMethod = generatedMethod;
            this._instanceFrameProvider = instanceFrameProvider;
        }

        /// <inheritdoc />
        public Variable TryFindVariable(IMethodVariables variables, Type type)
        {
            return this._instanceFrameProvider
                .TryGetVariableFromContainer<object>(this._generatedMethod.GeneratedType, type)
                ?.InstanceVariable;
        }
    }
}
