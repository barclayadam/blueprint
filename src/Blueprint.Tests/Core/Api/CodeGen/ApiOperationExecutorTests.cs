using System;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Middleware;
using Blueprint.StructureMap;
using FluentAssertions;
using NUnit.Framework;
using StructureMap;

namespace Blueprint.Tests.Core.Api.CodeGen
{
    public class ApiOperationExecutorTests
    {
        [Test]
        public async Task When_Handler_For_Operation_Exists_In_Container_Then_Call_Invoke_Method()
        {
            // Arrange
            var handler = new TestApiOperationHandler();
            var container = new Container(e => { e.For<IApiOperationHandler<TestApiOperation>>().Use(handler); });

            var apiOperationExecutor = CreateExecutor(container, (o) =>
            {
                o.WithApplicationName("Blueprint.Tests");

                o.UseMiddlewareBuilder<LoggingMiddlewareBuilder>();
                o.UseMiddlewareBuilder<OperationExecutorMiddlewareBuilder>();
                o.UseMiddlewareBuilder<FormatterMiddlewareBuilder>();

                o.AddOperation<TestApiOperation>();
            });

            var ctx = TestApiOperation.NewOperationContext(container);

            // Act
            await apiOperationExecutor.ExecuteAsync(ctx);

            // Assert
            handler.WasCalled.Should().BeTrue();
            handler.OperationPassed.Should().Be(ctx.Operation);

            // Simple check for a context that looks right, not extensive testing here!
            handler.ContextPassed.Should().NotBeNull();
            handler.ContextPassed.Should().Be(ctx);
        }

        private CodeGennedExecutor CreateExecutor(Container container, Action<BlueprintApiOptions> configure)
        {
            return new ApiOperationExecutorBuilder().Build(new BlueprintApiOptions(configure), container);
        }
    }
}
