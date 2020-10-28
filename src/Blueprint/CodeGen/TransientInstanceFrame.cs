using System;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.CodeGen
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that will resolve, from the registered <see cref="IServiceProvider"/>, an instance
    /// of a given type and store in a new <see cref="GetInstanceFrame{T}.InstanceVariable"/>.
    /// </summary>
    /// <typeparam name="T">The type of instance represented by this frame.</typeparam>
    public class TransientInstanceFrame<T> : GetInstanceFrame<T>
    {
        private readonly Type constructedType;

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

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"var {InstanceVariable} = services.{nameof(ServiceProviderServiceExtensions.GetRequiredService)}<{constructedType.Name}>();";
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            var serviceProviderVariable = variables.FindVariable(typeof(IServiceProvider));

            // We import this namespace to allow using extension method invocation, which cleans up the
            // generated code quite a bit for this use case.
            method.GeneratedType.Namespaces.Add(typeof(ServiceProviderServiceExtensions).Namespace);

            writer.WriteLine(
                $"var {InstanceVariable} = {serviceProviderVariable}.{nameof(ServiceProviderServiceExtensions.GetRequiredService)}<{constructedType.FullNameInCode()}>();");

            next();
        }
    }
}
