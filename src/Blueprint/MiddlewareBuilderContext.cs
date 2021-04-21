using System;
using System.Collections.Generic;
using System.Reflection;
using Blueprint.CodeGen;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint
{
    /// <summary>
    /// The context used when building a single pipeline method for a given operation, providing access to the data model
    /// of the operation and all associated data required to build up the frames for the pipeline.
    /// </summary>
    public class MiddlewareBuilderContext
    {
        private readonly Dictionary<Type, List<Func<Variable, IEnumerable<Frame>>>> _exceptionHandlers = new Dictionary<Type, List<Func<Variable, IEnumerable<Frame>>>>();
        private readonly FramesCollection _finallyFrames = new FramesCollection();

        private readonly InstanceFrameProvider _instanceFrameProvider;

        internal MiddlewareBuilderContext(
            GeneratedMethod executeMethod,
            ApiOperationDescriptor descriptor,
            ApiDataModel model,
            IServiceProvider serviceProvider,
            InstanceFrameProvider instanceFrameProvider)
        {
            this.ExecuteMethod = executeMethod;
            this.Descriptor = descriptor;
            this.Model = model;
            this.ServiceProvider = serviceProvider;

            this._instanceFrameProvider = instanceFrameProvider;
        }

        /// <summary>
        /// Gets the <see cref="GeneratedMethod" /> that is being built to handle the execution of
        /// an operation of the type described in the <see cref="Descriptor"/>.
        /// </summary>
        public GeneratedMethod ExecuteMethod { get; }

        /// <summary>
        /// Gets the generated type that contains the execution method and implements <see cref="IOperationExecutorPipeline" />.
        /// </summary>
        public GeneratedType GeneratedType => this.ExecuteMethod.GeneratedType;

        /// <summary>
        /// Gets the assembly that contains the generated pipeline.
        /// </summary>
        public GeneratedAssembly GeneratedAssembly => this.GeneratedType.GeneratedAssembly;

        /// <summary>
        /// Gets the descriptor for the operation that a method is currently being generated for.
        /// </summary>
        public ApiOperationDescriptor Descriptor { get; }

        /// <summary>
        /// Gets the model that represents the API.
        /// </summary>
        public ApiDataModel Model { get; }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the currently registered exception handlers.
        /// </summary>
        public IReadOnlyDictionary<Type, List<Func<Variable, IEnumerable<Frame>>>> ExceptionHandlers => this._exceptionHandlers;

        /// <summary>
        /// Gets the list of registered frames to execute in the operation's finally block.
        /// </summary>
        public IReadOnlyList<Frame> FinallyFrames => this._finallyFrames;

        /// <summary>
        /// Adds a reference to the given <see cref="Assembly" /> to the generated assembly, ensuring that any types that are used
        /// within the generated source code is available.
        /// </summary>
        /// <param name="assembly">The assembly to reference.</param>
        public void AddAssemblyReference(Assembly assembly)
        {
            Guard.NotNull(nameof(assembly), assembly);

            this.GeneratedAssembly.ReferenceAssembly(assembly);
        }

        /// <summary>
        /// Appends the given <see cref="Frame"/>s to the execution method (at <see cref="ExecuteMethod"/>).
        /// </summary>
        /// <param name="frames">The frames to be appended.</param>
        public void AppendFrames(params Frame[] frames)
        {
            this.ExecuteMethod.Frames.Append(frames);
        }

        /// <summary>
        /// Registers a handler for a given exception type, a set of frames that will make up the catch clause
        /// of an all-surrounding try-block for the whole operation pipeline.
        /// </summary>
        /// <remarks>
        /// This is useful for providing more specific processing of known exceptions, for example a <c>ValidationException</c>, where
        /// the overall exception strategy is not appropriate. In the case of validations for example, a HTTP response code of 422 makes
        /// more sense in a HTTP concept than the generic 500 failure.
        /// </remarks>
        /// <param name="exceptionType">The type of exception to handle (this is the exact type output to the generated code, meaning this
        /// type and everything that inherits it will be caught).</param>
        /// <param name="create">A method to create the frames, passed a <see cref="Variable"/> that will be of the
        /// specified exception type and represents the caught exception.</param>
        public void RegisterUnhandledExceptionHandler(Type exceptionType, Func<Variable, IEnumerable<Frame>> create)
        {
            this.AddAssemblyReference(exceptionType.Assembly);

            if (!this._exceptionHandlers.TryGetValue(exceptionType, out var handlers))
            {
                this._exceptionHandlers[exceptionType] = handlers = new List<Func<Variable, IEnumerable<Frame>>>();
            }

            handlers.Add(create);
        }

        /// <summary>
        /// Registers one or more frames that should be rendered in a "finally" block that surrounds all of the execution of an operation,
        /// allowing middleware to always act at the end of an operation (useful for things like logging and auditing).
        /// </summary>
        /// <param name="frames">The frame(s) to append.</param>
        public void RegisterFinallyFrames(params Frame[] frames)
        {
            this._finallyFrames.Append(frames);
        }

        /// <summary>
        /// Gets the variable of the given type that is a property of the <see cref="ApiOperationContext" /> variable
        /// passed in to the pipeline handler method.
        /// </summary>
        /// <param name="type">The type of the variable to grab.</param>
        /// <returns>The <see cref="Variable"/> representing the given type.</returns>
        /// <seealso cref="ApiOperationContextVariableSource" />
        public Variable FindVariable(Type type)
        {
            return this.ExecuteMethod.FindVariable(type);
        }

        /// <summary>
        /// Gets the variable of the given type that is a property of the <see cref="ApiOperationContext" /> variable
        /// passed in to the pipeline handler method.
        /// </summary>
        /// <typeparam name="T">The type of the variable to grab.</typeparam>
        /// <returns>The <see cref="Variable"/> representing the given type.</returns>
        /// <seealso cref="ApiOperationContextVariableSource" />
        public Variable FindVariable<T>()
        {
            return this.ExecuteMethod.FindVariable(typeof(T));
        }

        /// <summary>
        /// Generates a <see cref="GetInstanceFrame{T}" /> and associated <see cref="Variable" /> for getting
        /// an instance from the current request container.
        /// </summary>
        /// <remarks>
        /// This method attempts to optimise the output by looking at the registrations in the container, checking
        /// for singletons etc. and turning them in to injected fields to avoid the lookup per request.
        /// </remarks>
        /// <remarks>
        /// Note that this will automatically add a reference to the assembly of the specified type to the generated
        /// assembly.
        /// </remarks>
        /// <typeparam name="T">The type of the instance to load.</typeparam>
        /// <returns>A frame (that needs to be added to the method) representing the container.GetInstance call.</returns>
        /// <exception cref="InvalidOperationException">If no registration exists for the given type.</exception>>
        public GetInstanceFrame<T> VariableFromContainer<T>()
        {
            return this.GetVariableFromContainer<T>(typeof(T));
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
        /// <exception cref="InvalidOperationException">If no registration exists for the given type.</exception>>
        public GetInstanceFrame<object> VariableFromContainer(Type type)
        {
            return this.GetVariableFromContainer<object>(type);
        }

        /// <summary>
        /// Tries to generate a <see cref="GetInstanceFrame{T}" /> and associated <see cref="Variable" /> for getting
        /// an instance from the current request container.
        /// </summary>
        /// <remarks>
        /// This method attempts to optimise the output by looking at the registrations in the container, checking
        /// for singletons etc. and turning them in to injected fields to avoid the lookup per request.
        /// </remarks>
        /// <remarks>
        /// Note that this will automatically add a reference to the assembly of the specified type to the generated
        /// assembly.
        /// </remarks>
        /// <typeparam name="T">The type of the instance to load.</typeparam>
        /// <returns>A frame (that needs to be added to the method) representing the container.GetInstance call, or
        /// <c>null</c> if no such registration exists.</returns>
        public GetInstanceFrame<T> TryGetVariableFromContainer<T>()
        {
            return this.TryGetVariableFromContainer<T>(typeof(T));
        }

        /// <summary>
        /// Tries to generate a <see cref="GetInstanceFrame{T}" /> and associated <see cref="Variable" /> for getting
        /// an instance from the current request container.
        /// </summary>
        /// <remarks>
        /// This method attempts to optimise the output by looking at the registrations in the container, checking
        /// for singletons etc. and turning them in to injected fields to avoid the lookup per request.
        /// </remarks>
        /// <param name="type">The type of the instance to load.</param>
        /// <returns>A frame (that needs to be added to the method) representing the container.GetInstance call, or
        /// <c>null</c> if no such registration exists.</returns>
        public GetInstanceFrame<object> TryGetVariableFromContainer(Type type)
        {
            return this.TryGetVariableFromContainer<object>(type);
        }

        private GetInstanceFrame<T> GetVariableFromContainer<T>(Type type)
        {
            this.AddAssemblyReference(type.Assembly);

            return this._instanceFrameProvider.GetVariableFromContainer<T>(this.GeneratedType, type);
        }

        private GetInstanceFrame<T> TryGetVariableFromContainer<T>(Type type)
        {
            this.AddAssemblyReference(type.Assembly);

            return this._instanceFrameProvider.TryGetVariableFromContainer<T>(this.GeneratedType, type);
        }
    }
}
