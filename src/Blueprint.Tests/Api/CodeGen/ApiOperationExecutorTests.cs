using System;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Api.Middleware;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.Api.CodeGen
{
    public class ApiOperationExecutorTests
    {
        [Test]
        public async Task When_Handler_For_Operation_Exists_In_Container_Then_Call_Invoke_Method()
        {
            // Arrange
            var handler = new TestApiOperationHandler();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IApiOperationHandler<TestApiOperation>>(handler);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var apiOperationExecutor = CreateApiOperationExecutor(serviceProvider, (o) =>
            {
                o.WithApplicationName("Blueprint.Tests");

                o.UseMiddlewareBuilder<LoggingMiddlewareBuilder>();
                o.UseMiddlewareBuilder<OperationExecutorMiddlewareBuilder>();
                o.UseMiddlewareBuilder<FormatterMiddlewareBuilder>();

                o.AddOperation<TestApiOperation>();
            });

            var ctx = TestApiOperation.NewOperationContext(serviceProvider);

            // Act
            await apiOperationExecutor.ExecuteAsync(ctx);

            // Assert
            handler.WasCalled.Should().BeTrue();
            handler.OperationPassed.Should().Be(ctx.Operation);

            // Simple check for a context that looks right, not extensive testing here!
            handler.ContextPassed.Should().NotBeNull();
            handler.ContextPassed.Should().Be(ctx);
        }

        private static CodeGennedExecutor CreateApiOperationExecutor(IServiceProvider serviceProvider, Action<BlueprintApiOptions> configure)
        {
            return new ApiOperationExecutorBuilder().Build(new BlueprintApiOptions(configure), serviceProvider);
        }
    }
}
