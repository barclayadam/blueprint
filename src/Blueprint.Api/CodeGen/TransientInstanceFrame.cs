using System;
using System.Collections.Generic;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.CodeGen
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that will resolve, from the registered <see cref="IServiceProvider"/>, an instance
    /// of a given type and store in a new <see cref="GetInstanceFrame{T}.InstanceVariable"/>.
    /// </summary>
    /// <typeparam name="T">The type of instance represented by this frame.</typeparam>
    public class TransientInstanceFrame<T> : GetInstanceFrame<T>
    {
        private readonly Type constructedType;
        private Variable serviceProviderVariable;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientInstanceFrame{T}"/> class with the specified
        /// variable type, which is the type that will be requested from the container at runtime.
        /// </summary>
        /// <param name="variableType">The type of variable and request container type.</param>
        public TransientInstanceFrame(Type variableType)
        {
            constructedType = variableType;
            InstanceVariable = new Variable(variableType, this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientInstanceFrame{T}"/> class with the specified
        /// variable type, and a (usually) more specific type that would be requested from the container.
        /// </summary>
        /// <remarks>
        /// This is a small readability optimisation that can be used when we know that only possible type that
        /// could satisfy a requested interface type (i.e. requested IFoo that is only implemented by Foo, so
        /// <paramref name="variableType"/> is <c>IFoo</c> but <paramref name="constructedType" /> would
        /// be <c>Foo</c> and generated code would be <c>var i = container.GetInstance&lt;Foo&gt;()</c>.
        /// </remarks>
        /// <param name="variableType">The type of variable and request container type.</param>
        /// <param name="constructedType">The type to be requested from the container.</param>
        public TransientInstanceFrame(Type variableType, Type constructedType)
        {
            this.constructedType = constructedType;
            InstanceVariable = new Variable(variableType, this);
        }

        /// <inheritdoc />
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            // We import this namespace to allow using extension method invocation, which cleans up the
            // generated code quite a bit for this use case.
            method.GeneratedType.Namespaces.Add(typeof(ServiceProviderServiceExtensions).Namespace);

            writer.Write(
                $"var {InstanceVariable} = {serviceProviderVariable}.{nameof(ServiceProviderServiceExtensions.GetRequiredService)}<{constructedType.FullNameInCode()}>();");

            Next?.GenerateCode(method, writer);
        }

        /// <inheritdoc />
        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            serviceProviderVariable = chain.FindVariable(typeof(IServiceProvider));

            yield return serviceProviderVariable;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"var {InstanceVariable} = {serviceProviderVariable}.{nameof(ServiceProviderServiceExtensions.GetRequiredService)}<{constructedType.Name}>();";
        }
    }
}
