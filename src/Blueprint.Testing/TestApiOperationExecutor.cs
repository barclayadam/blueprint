using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blueprint.Testing
{
    /// <summary>
    /// Implements an <see cref="IApiOperationExecutor"/> that can be set up in tests as an executor that sets defaults on configuration options
    /// and provides an easier means of testing pipelines and middleware builders.
    /// </summary>
    /// <remarks>
    /// Instances of this class can be created using the static factory method <see cref="Create"/>.
    /// </remarks>
    public class TestApiOperationExecutor : IApiOperationExecutor
    {
        private readonly ServiceProvider serviceProvider;
        private readonly CodeGennedExecutor executor;

        private TestApiOperationExecutor(ServiceProvider serviceProvider, CodeGennedExecutor executor)
        {
            this.serviceProvider = serviceProvider;
            this.executor = executor;
        }

        /// <inheritdoc />
        public ApiDataModel DataModel => executor.DataModel;

        /// <summary>
        /// Creates a new <see cref="TestApiOperationExecutor" /> with the specified configuration which allows adding static handlers
        /// for operations and configuring the middleware pipeline.
        /// </summary>
        /// <param name="configure">An action that will configure the pipeline for the given test.</param>
        /// <returns>A new executor with the specified options combined with sensible defaults for tests.</returns>
        public static TestApiOperationExecutor Create(Action<TestApiOperationExecutorBuilder> configure)
        {
            var collection = new ServiceCollection();

            var builder = new TestApiOperationExecutorBuilder(collection);
            configure(builder);

            collection.AddLogging(b => b
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug));

            collection.AddBlueprintApi(o => o
                .SetApplicationName("Blueprint.Tests")
                .Compilation(r => r
                    .UseOptimizationLevel(OptimizationLevel.Debug)
                    .UseCompileStrategy<InMemoryOnlyCompileStrategy>())
                .Pipeline(builder.PipelineConfigurer)
                .AddOperations(builder.OperationTypes));

            var serviceProvider = collection.BuildServiceProvider();
            var executor = (CodeGennedExecutor)serviceProvider.GetRequiredService<IApiOperationExecutor>();

            return new TestApiOperationExecutor(serviceProvider, executor);
        }

        /// <summary>
        /// Gets all of the code that was used to generate this executor.
        /// </summary>
        /// <returns>The code used to create all executors.</returns>
        public string WhatCodeDidIGenerate()
        {
            return executor.WhatCodeDidIGenerate();
        }

        /// <summary>
        /// Gets the code that was used to generate the executor for the operation specified by <paramref name="operationType" />.
        /// </summary>
        /// <param name="operationType">The operation type to get source code for.</param>
        /// <returns>The executor's source code.</returns>
        public string WhatCodeDidIGenerateFor(Type operationType)
        {
            return executor.WhatCodeDidIGenerateFor(operationType);
        }

        /// <summary>
        /// Gets the code that was used to generate the executor for the operation specified by <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The operation type to get source code for.</typeparam>
        /// <returns>The executor's source code.</returns>
        public string WhatCodeDidIGenerateFor<T>() where T : IApiOperation
        {
            return executor.WhatCodeDidIGenerateFor<T>();
        }

        /// <summary>
        /// Creates and configures a new <see cref="ApiOperationContext" /> for an operation of the specified generic
        /// type.
        /// </summary>
        /// <typeparam name="T">The type of operation to create a context for.</typeparam>
        /// <returns>A newly configured <see cref="ApiOperationContext" />.</returns>
        public ApiOperationContext HttpContextFor<T>() where T : IApiOperation
        {
            var context = DataModel.CreateOperationContext(serviceProvider, typeof(T));
            context.ConfigureHttp("https://www.my-api.com/api/" + typeof(T));

            return context;
        }

        /// <inheritdoc />
        public Task<OperationResult> ExecuteAsync(ApiOperationContext context)
        {
            return executor.ExecuteAsync(context);
        }

        /// <inheritdoc />
        public Task<OperationResult> ExecuteWithNewScopeAsync<T>(T operation) where T : IApiOperation
        {
            return executor.ExecuteWithNewScopeAsync(operation);
        }

        public class TestApiOperationExecutorBuilder
        {
            private readonly ServiceCollection collection;
            private Action<BlueprintMiddlewareConfigurer> pipelineConfigurer = c => {};

            internal TestApiOperationExecutorBuilder(ServiceCollection collection)
            {
                this.collection = collection;
            }

            internal List<Type> OperationTypes { get; } = new List<Type>();

            internal Action<BlueprintMiddlewareConfigurer> PipelineConfigurer => pipelineConfigurer;

            /// <summary>
            /// Called back with the <see cref="ServiceCollection" /> that is to be used by the operator that is being
            /// built, allowing customisation and further registrations of the DI container to be built.
            /// </summary>
            /// <param name="action">The action to be immediately called to configure the <see cref="ServiceCollection"/>.</param>
            /// <returns>This instance.</returns>
            public TestApiOperationExecutorBuilder WithServices(Action<ServiceCollection> action)
            {
                action(collection);

                return this;
            }

            /// <summary>
            /// Configures a new handler, which will also implicitly register the operation of type <typeparamref name="T"/>
            /// with the <see cref="ApiDataModel" /> of the executor.
            /// </summary>
            /// <param name="handler">The handler to register.</param>
            /// <typeparam name="T">The (usually inferred) type of operation for this handler.</typeparam>
            /// <returns>This instance.</returns>
            public TestApiOperationExecutorBuilder WithHandler<T>(IApiOperationHandler<T> handler) where T : IApiOperation
            {
                collection.AddSingleton(handler);

                OperationTypes.Add(typeof(T));

                return this;
            }

            public TestApiOperationExecutorBuilder Pipeline(Action<BlueprintMiddlewareConfigurer> configurer)
            {
                this.pipelineConfigurer = configurer;

                return this;
            }
        }
    }
}
