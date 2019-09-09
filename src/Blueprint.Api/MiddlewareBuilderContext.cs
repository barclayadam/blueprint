using System;
using System.Collections.Generic;
using Blueprint.Api.CodeGen;
using Blueprint.Api.Validation;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Api
{
    public class MiddlewareBuilderContext
    {
        private readonly Dictionary<Type, Func<Variable, IEnumerable<Frame>>> exceptionHandlers = new Dictionary<Type, Func<Variable, IEnumerable<Frame>>>();

        public MiddlewareBuilderContext(
            ApiOperationContextVariableSource apiContextVariableSource,
            GeneratedType generatedType,
            GeneratedMethod executeMethod,
            ApiOperationDescriptor descriptor,
            ApiDataModel model,
            IInstanceFrameProvider instanceFrameProvider,
            IValidationSourceBuilder[] validationSourceBuilders)
        {
            ApiContextVariableSource = apiContextVariableSource;
            GeneratedType = generatedType;
            ExecuteMethod = executeMethod;
            Descriptor = descriptor;
            Model = model;
            InstanceFrameProvider = instanceFrameProvider;
            ValidationSourceBuilders = validationSourceBuilders;
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
        /// Gets the model that represents the API.
        /// </summary>
        public ApiDataModel Model { get; }

        /// <summary>
        /// Gets the instance frame provider.
        /// </summary>
        public IInstanceFrameProvider InstanceFrameProvider { get; }

        /// <summary>
        /// Gets the validation source builders.
        /// </summary>
        public IValidationSourceBuilder[] ValidationSourceBuilders { get; set; }
        
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
            return InstanceFrameProvider.VariableFromContainer<T>(GeneratedType, typeof(T));
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
            return InstanceFrameProvider.VariableFromContainer<object>(GeneratedType, type);
        }
    }
}
