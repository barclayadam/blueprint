using System;
using System.Collections.Generic;

using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

using StructureMap;

namespace Blueprint.Core.Api.CodeGen
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that will resolve, from the registered <see cref="IContainer"/>, an instance
    /// of a given type and store in a new <see cref="GetInstanceFrame{T}.InstanceVariable"/>.
    /// </summary>
    public class TransientInstanceFrame<T> : GetInstanceFrame<T>
    {
        private readonly Type containerConstructedType;
        private Variable containerVariable;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientInstanceFrame{T}"/> class with the specified
        /// variable type, which is the type that will be requested from the container at runtime.
        /// </summary>
        /// <param name="variableType">The type of variable and request container type.</param>
        public TransientInstanceFrame(Type variableType)
        {
            containerConstructedType = variableType;
            InstanceVariable = new Variable(variableType, this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientInstanceFrame{T}"/> class with the specified
        /// variable type, and a (usually) more specific type that would be requested from the container.
        /// </summary>
        /// <remarks>
        /// This is a small readability optimisation that can be used when we know that only possible type that
        /// could satisfy a requested interface type (i.e. requested IFoo that is only implemented by Foo, so
        /// <paramref name="variableType"/> is <c>IFoo</c> but <paramref name="containerConstructedType" /> would
        /// be <c>Foo</c> and generated code would be <c>var i = container.GetInstance<Foo>()</c>.
        /// </remarks>
        /// <param name="variableType">The type of variable and request container type.</param>
        /// <param name="containerConstructedType">The type to be requested from the container.</param>
        public TransientInstanceFrame(Type variableType, Type containerConstructedType)
        {
            this.containerConstructedType = containerConstructedType;
            InstanceVariable = new Variable(variableType, this);
        }

        /// <inheritdoc />
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            // TODO: This may not be correct depending on the lifecycle. Refactor the creation of instances
            // to allow this frame to have access to the container and therefore make decisions based on the configured lifecycle and
            // potentially even look at the registered instances and make these same optimisations if only a single instance has been
            // declared for the interface type.

            // If the type that we want to create is a concrete class with no public constructors then
            // we can new up directly and avoid any container overhead
            if (HasNoDeclaredConstructors(containerConstructedType))
            {
                writer.Write(
                    $"var {InstanceVariable} = new {containerConstructedType.FullNameInCode()}();");
            }
            else
            {
                writer.Write(
                    $"var {InstanceVariable} = {containerVariable}.{nameof(IContainer.GetInstance)}<{containerConstructedType.FullNameInCode()}>();");
            }

            Next?.GenerateCode(method, writer);
        }

        /// <inheritdoc />
        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            containerVariable = chain.FindVariable(typeof(IContainer));

            yield return containerVariable;
        }

        private static bool HasNoDeclaredConstructors(Type type)
        {
            return type.IsClass && type.IsAbstract == false && type.GetConstructors().Length == 1 && type.GetConstructors()[0].GetParameters().Length == 0;
        }
    }
}
