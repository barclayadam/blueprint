using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Middleware;
using Blueprint.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly IApiOperationExecutor executor;

        private TestApiOperationExecutor(IApiOperationExecutor executor)
        {
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
        public static IApiOperationExecutor Create(Action<TestApiOperationExecutorBuilder> configure)
        {
            var collection = new ServiceCollection();

            var builder = new TestApiOperationExecutorBuilder(collection);
            configure(builder);

            collection.AddBlueprintApi(o =>
            {
                o.Rules.OptimizationLevel = OptimizationLevel.Debug;
                o.Rules.UseCompileStrategy<InMemoryOnlyCompileStrategy>();

                o.WithApplicationName("Blueprint.Tests");

                foreach (var middlewareType in builder.MiddlewareBuilderTypes)
                {
                    o.UseMiddlewareBuilder(middlewareType);
                }

                o.UseMiddlewareBuilder<OperationExecutorMiddlewareBuilder>();
                o.UseMiddlewareBuilder<FormatterMiddlewareBuilder>();

                foreach (var operationType in builder.OperationTypes)
                {
                    o.AddOperation(operationType);
                }
            });

            var serviceProvider = collection.BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IApiOperationExecutor>();

            return new TestApiOperationExecutor(executor);
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

            internal TestApiOperationExecutorBuilder(ServiceCollection collection)
            {
                this.collection = collection;
            }

            internal List<Type> OperationTypes { get; } = new List<Type>();

            internal List<Type> MiddlewareBuilderTypes { get; } = new List<Type>();

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

            /// <summary>
            /// Configures the pipeline to use the middleware builder specified by the type parameter <typeparamref name="T" />.
            /// </summary>
            /// <typeparam name="T">The type of <see cref="IMiddlewareBuilder"/> to register.</typeparam>
            /// <returns>This instance.</returns>
            public TestApiOperationExecutorBuilder WithMiddleware<T>() where T : IMiddlewareBuilder
            {
                MiddlewareBuilderTypes.Add(typeof(T));

                return this;
            }
        }
    }
}
