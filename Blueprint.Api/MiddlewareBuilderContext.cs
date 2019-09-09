using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Api.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using StructureMap;
using StructureMap.Pipeline;

namespace Blueprint.Api
{
    public class MiddlewareBuilderContext
    {
        private readonly Dictionary<Type, Func<Variable, IEnumerable<Frame>>> exceptionHandlers = new Dictionary<Type, Func<Variable, IEnumerable<Frame>>>();

        internal MiddlewareBuilderContext(
            ApiOperationContextVariableSource apiContextVariableSource,
            GeneratedType generatedType,
            GeneratedMethod executeMethod,
            ApiOperationDescriptor descriptor,
            IContainer container,
            ApiDataModel model)
        {
            ApiContextVariableSource = apiContextVariableSource;
            GeneratedType = generatedType;
            ExecuteMethod = executeMethod;
            Descriptor = descriptor;
            Container = container;
            Model = model;
        }

        /// <summary>
        /// An <see cref="IVariableSource" /> that should be used to grab any variables that would
        /// be found on the <see cref="ApiOperationContext" />.
        /// </summary>
        public ApiOperationContextVariableSource ApiContextVariableSource { get; }

        /// <summary>
        /// Gets the generated type that contains the execution method and implements <see cref="IOperationExecutorPipeline" />.
        /// </summary>
        public GeneratedType GeneratedType { get; }

        /// <summary>
        /// Gets the <see cref="GeneratedMethod" /> that is being built to handle the execution of
        /// an operation of the type described in the <see cref="Descriptor"/>.
        /// </summary>
        public GeneratedMethod ExecuteMethod { get; }

        /// <summary>
        /// Gets the descriptor for the operation that a method is currently being generated for.
        /// </summary>
        public ApiOperationDescriptor Descriptor { get; }

        /// <summary>
        /// Gets the root container under which operations will execute. This value represents a configured container,
        /// and _not_ the one that is passed on each execution as that will be a nested container.
        /// </summary>
        public IContainer Container { get; }

        /// <summary>
        /// Gets the model that represents the API.
        /// </summary>
        public ApiDataModel Model { get; }

        /// <summary>
        /// Gets the currently registered exception handlers.
        /// </summary>
        public IReadOnlyDictionary<Type, Func<Variable, IEnumerable<Frame>>> ExceptionHandlers => exceptionHandlers;

        /// <summary>
        /// Appends the given <see cref="Frame"/>s to the execution method (at <see cref="ExecuteMethod"/>).
        /// </summary>
        /// <param name="frames">The frames to be appended.</param>
        public void AppendFrames(params Frame[] frames)
        {
            ExecuteMethod.Frames.Append(frames);
        }

        /// <summary>
        /// Registers a handler for a given exception type, a set of frames that will make up the catch clause
        /// of an all-surrounding try-block for the whole operation pipeline.
        /// </summary>
        /// <param name="exceptionType">The type of exception to handle.</param>
        /// <param name="create">A method to create the frames, passed a <see cref="Variable"/> that will be of the
        /// specified exception type and represents the caught exception.</param>
        public void RegisterUnhandledExceptionHandler(Type exceptionType, Func<Variable, IEnumerable<Frame>> create)
        {
            exceptionHandlers[exceptionType] = create;
        }

        /// <summary>
        /// Gets the variable of the given type that is a property of the <see cref="ApiOperationContext" /> variable
        /// passed in to the pipeline handler method.
        /// </summary>
        /// <param name="type">The type of the variable to grab.</param>
        /// <returns>The <see cref="Variable"/> representing the given type.</returns>
        /// <seealso cref="ApiOperationContextVariableSource" />
        public Variable VariableFromContext(Type type)
        {
            return ApiContextVariableSource.Get(type);
        }

        /// <summary>
        /// Gets the variable of the given type that is a property of the <see cref="ApiOperationContext" /> variable
        /// passed in to the pipeline handler method.
        /// </summary>
        /// <typeparam name="T">The type of the variable to grab.</typeparam>
        /// <returns>The <see cref="Variable"/> representing the given type.</returns>
        /// <seealso cref="ApiOperationContextVariableSource" />
        public Variable VariableFromContext<T>()
        {
            return ApiContextVariableSource.Get(typeof(T));
        }

        /// <summary>
        /// Generates a <see cref="GetInstanceFrame{T}" /> and associated <see cref="Variable" /> for getting
        /// an instance from the current request container.
        /// </summary>
        /// <remarks>
        /// This method attempts to optimise the output by looking at the registrations in the container, checking
        /// for singletons etc. and turning them in to injected fields to avoid the lookup per request.
        /// </remarks>
        /// <typeparam name="T">The type of the instance to load.</typeparam>
        /// <returns>A frame (that needs to be added to the method) representing the container.GetInstance call.</returns>
        public GetInstanceFrame<T> VariableFromContainer<T>()
        {
            return VariableFromContainer<T>(typeof(T));
        }

        /// <summary>
        /// Generates a <see cref="GetInstanceFrame{T}" /> and associated <see cref="Variable" /> for getting
        /// an instance from the current request container.
        /// </summary>
        /// <remarks>
        /// This method attempts to optimise the output by looking at the registrations in the container, checking
        /// for singletons etc. and turning them in to injected fields to avoid the lookup per request.
        /// </remarks>
        /// <param name="type">The type of the instance to load.</param>
        /// <returns>A frame (that needs to be added to the method) representing the container.GetInstance call.</returns>
        public GetInstanceFrame<object> VariableFromContainer(Type type)
        {
            return VariableFromContainer<object>(type);
        }

        private GetInstanceFrame<T> VariableFromContainer<T>(Type toLoad)
        {
            var config = Container.Model.For(toLoad);

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

                    GeneratedType.AllInjectedFields.Add(injected);

                    return new InjectedFrame<T>(injected);
                }

                // Small tweak to resolve the actual known type. Makes generated code a little nicer as it
                // makes it obvious what is _actually_ going to be built without knowledge of the container
                // setup
                return new TransientInstanceFrame<T>(toLoad, instanceRef.ReturnedType);
            }

            return new TransientInstanceFrame<T>(toLoad);
        }

        private class InjectedFrame<T> : GetInstanceFrame<T>
        {
            public InjectedFrame(InjectedField field)
            {
                InstanceVariable = field;
            }

            public override IEnumerable<Variable> Creates => new[] { InstanceVariable };

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                // DO nothing here, we need to have this class so we can return a GetInstanceFrame
                // instance, but the actual variable is injected and therefore we need no code output
                Next?.GenerateCode(method, writer);
            }
        }
    }
}
