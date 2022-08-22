using System;
using System.Threading;
using System.Threading.Tasks;
using Blueprint.Configuration;
using Blueprint.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
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
        private readonly ServiceProvider _serviceProvider;
        private readonly CodeGennedExecutor _executor;

        private TestApiOperationExecutor(ServiceProvider serviceProvider, CodeGennedExecutor executor)
        {
            this._serviceProvider = serviceProvider;
            this._executor = executor;
        }

        /// <inheritdoc />
        public ApiDataModel DataModel => this._executor.DataModel;

        /// <summary>
        /// Creates a new <see cref="TestApiOperationExecutor" /> with the specified configuration which allows adding static handlers
        /// for operations and configuring the middleware pipeline.
        /// </summary>
        /// <param name="configure">An action that will configure the pipeline for the given test.</param>
        /// <param name="configureServices">Configures the used <see cref="IServiceCollection" />.</param>
        /// <param name="configureHttp">An optional action that can configure <see cref="BlueprintHttpOptions" />, executed
        /// <c>after</c> the default configuration has been run.</param>
        /// <returns>A new executor with the specified options combined with sensible defaults for tests.</returns>
        public static TestApiOperationExecutor CreateHttp(
            Action<BlueprintApiBuilder> configure,
            Action<IServiceCollection> configureServices = null,
            Action<BlueprintHttpBuilder> configureHttp = null)
        {
            return Create(b => b.Http(configureHttp), configure, configureServices);
        }

        /// <summary>
        /// Creates a new <see cref="TestApiOperationExecutor" /> with the specified configuration which allows adding static handlers
        /// for operations and configuring the middleware pipeline.
        /// </summary>
        /// <param name="configure">An action that will configure the pipeline for the given test.</param>
        /// <param name="configureServices">Configures the used <see cref="IServiceCollection" />.</param>
        /// <returns>A new executor with the specified options combined with sensible defaults for tests.</returns>
        public static TestApiOperationExecutor CreateStandalone(
            Action<BlueprintApiBuilder> configure,
            Action<IServiceCollection> configureServices = null)
        {
            return Create(b => b, configure, configureServices);
        }

        /// <summary>
        /// Creates a new <see cref="TestApiOperationExecutor" /> with the specified configuration which allows adding static handlers
        /// for operations and configuring the middleware pipeline.
        /// </summary>
        /// <param name="createHost">A delegate to create the host that will be used.</param>
        /// <param name="configure">An action that will configure the pipeline for the given test.</param>
        /// <param name="configureServices">Configures the used <see cref="IServiceCollection" />.</param>
        /// <returns>A new executor with the specified options combined with sensible defaults for tests.</returns>
        public static TestApiOperationExecutor Create(
            Func<BlueprintApiBuilder, BlueprintApiBuilder> createHost,
            Action<BlueprintApiBuilder> configure,
            Action<IServiceCollection> configureServices = null)
        {
            var collection = new ServiceCollection();

            collection.AddLogging(b => b
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug));

            collection.AddBlueprintApi(b =>
            {
                var builder = createHost(b);

                builder
                    .Compilation(r => r
                        /* We want a unique DLL name every time, avoids clashes and ensures we always do
                           an actual build and compilation so we can get the generated code */
                        .UseInMemoryStrategy(Guid.NewGuid().ToString()));

                configure(builder);
            });

            collection.AddSingleton<IHostEnvironment>(new HostingEnvironment());

            configureServices?.Invoke(collection);

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
            return this._executor.WhatCodeDidIGenerate();
        }

        /// <summary>
        /// Gets the code that was used to generate the executor for the operation specified by <paramref name="operationType" />.
        /// </summary>
        /// <param name="operationType">The operation type to get source code for.</param>
        /// <returns>The executor's source code.</returns>
        public string WhatCodeDidIGenerateFor(Type operationType)
        {
            return this._executor.WhatCodeDidIGenerateFor(operationType);
        }

        /// <summary>
        /// Gets the code that was used to generate the executor for the operation specified by <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The operation type to get source code for.</typeparam>
        /// <returns>The executor's source code.</returns>
        public string WhatCodeDidIGenerateFor<T>()
        {
            return this._executor.WhatCodeDidIGenerateFor<T>();
        }

        /// <summary>
        /// Creates and configures a new <see cref="ApiOperationContext" /> for an operation of the specified generic
        /// type, adding HTTP-specific properties to the context.
        /// </summary>
        /// <param name="configureContext">An optional callback to further configure the HttpContext of the context.</param>
        /// <param name="token">A cancellation token to indicate the operation should stop.</param>
        /// <typeparam name="T">The type of operation to create a context for.</typeparam>
        /// <returns>A newly configured <see cref="ApiOperationContext" />.</returns>
        public ApiOperationContext HttpContextFor<T>(Action<HttpContext> configureContext = null, CancellationToken token = default)
        {
            var context = this.DataModel.CreateOperationContext(this._serviceProvider, typeof(T), token);
            var httpContext = context.ConfigureHttp("https://www.my-api.com/api/" + typeof(T));

            configureContext?.Invoke(httpContext);

            return context;
        }

        /// <summary>
        /// Creates and configures a new <see cref="ApiOperationContext" /> for an operation of the specified generic
        /// type, adding HTTP-specific properties to the context.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="configureContext">An optional callback to further configure the HttpContext of the context.</param>
        /// <param name="token">A cancellation token to indicate the operation should stop.</param>
        /// <typeparam name="T">The type of operation to create a context for.</typeparam>
        /// <returns>A newly configured <see cref="ApiOperationContext" />.</returns>
        public ApiOperationContext HttpContextFor<T>(T operation, Action<HttpContext> configureContext = null, CancellationToken token = default)
        {
            var context = this.DataModel.CreateOperationContext(this._serviceProvider, operation, token);
            var httpContext = context.ConfigureHttp("https://www.my-api.com/api/" + typeof(T));

            configureContext?.Invoke(httpContext);

            return context;
        }

        /// <summary>
        /// Creates and configures a new <see cref="ApiOperationContext" /> for an operation of the specified generic
        /// type.
        /// </summary>
        /// <param name="token">A cancellation token to indicate the operation should stop.</param>
        /// <typeparam name="T">The type of operation to create a context for.</typeparam>
        /// <returns>A newly configured <see cref="ApiOperationContext" />.</returns>
        public ApiOperationContext ContextFor<T>(CancellationToken token = default)
        {
            return this.DataModel.CreateOperationContext(this._serviceProvider, typeof(T), token);
        }

        /// <summary>
        /// Creates and configures a new <see cref="ApiOperationContext" /> for an operation of the specified generic
        /// type.
        /// </summary>
        /// <param name="operation">The API operation to create a context for.</param>
        /// <param name="token">A cancellation token to indicate the operation should stop.</param>
        /// <returns>A newly configured <see cref="ApiOperationContext" />.</returns>
        public ApiOperationContext ContextFor(object operation, CancellationToken token = default)
        {
            return this.DataModel.CreateOperationContext(this._serviceProvider, operation, token);
        }

        public async Task<OperationResult> ExecuteAsync<T>()
        {
            using var serviceScope = this._serviceProvider.CreateScope();
            var apiOperationContext = this.BuildContext<T>(serviceScope.ServiceProvider, CancellationToken.None);
            var result = await this._executor.ExecuteAsync(apiOperationContext);

            if (result is UnhandledExceptionOperationResult e)
            {
                e.Rethrow();
            }

            return result;
        }

        public async Task<OperationResult> ExecuteAsync(object operation)
        {
            using var serviceScope = this._serviceProvider.CreateScope();
            var apiOperationContext = this.BuildContext(serviceScope.ServiceProvider, operation, CancellationToken.None);
            var result = await this._executor.ExecuteAsync(apiOperationContext);

            if (result is UnhandledExceptionOperationResult e)
            {
                e.Rethrow();
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<OperationResult> ExecuteAsync(ApiOperationContext context)
        {
            var result = await this._executor.ExecuteAsync(context);

            if (result is UnhandledExceptionOperationResult e)
            {
                e.Rethrow();
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<OperationResult> ExecuteWithNewScopeAsync(object operation, CancellationToken token = default)
        {
            using var serviceScope = this._serviceProvider.CreateScope();
            var apiOperationContext = this.BuildContext(serviceScope.ServiceProvider, operation, token);

            var result = await this._executor.ExecuteAsync(apiOperationContext);

            if (result is UnhandledExceptionOperationResult e)
            {
                e.Rethrow();
            }

            return result;
        }

        public async Task<OperationResult> ExecuteWithNoUnwrapAsync(object operation, CancellationToken token = default)
        {
            return await this._executor.ExecuteWithNewScopeAsync(operation, token);
        }

        private ApiOperationContext BuildContext(IServiceProvider services, object operation, CancellationToken token)
        {
            var needsHttpContext = this._executor.DataModel.TryFindOperation(operation.GetType(), out var descriptor) &&
                                   descriptor.TryGetFeatureData<HttpOperationFeatureData>(out _);

            return needsHttpContext
                ? this.HttpContextFor(operation, token: token)
                : this.DataModel.CreateOperationContext(services, operation, token);
        }

        private ApiOperationContext BuildContext<T>(IServiceProvider services, CancellationToken token)
        {
            var operationType = typeof(T);

            var needsHttpContext = this._executor.DataModel.TryFindOperation(operationType, out var descriptor) &&
                                   descriptor.TryGetFeatureData<HttpOperationFeatureData>(out _);

            return needsHttpContext
                ? this.HttpContextFor<T>(token: token)
                : this.DataModel.CreateOperationContext(services, operationType, token);
        }
    }
}
