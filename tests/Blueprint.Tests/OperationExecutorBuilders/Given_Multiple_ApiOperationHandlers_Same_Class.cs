using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Blueprint.Middleware;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.OperationExecutorBuilders
{
    public class Given_Multiple_ApiOperationHandlers_Same_Class
    {
        [Test]
        public void When_Handler_Exists_In_Same_Assembly_Can_Find()
        {
            // Arrange
            var scanner = new ApiOperationHandlerExecutorBuilderScanner();

            var handlers = scanner.FindHandlers(
                new ServiceCollection(),
                typeof(OperationA),
                Enumerable.Empty<Assembly>());

            handlers.Should().NotBeEmpty();
        }

        [Test]
        public void When_Handler_Exists_In_Same_Assembly_And_Implements_Multiple_Interfaces_Can_Find_For_All()
        {
            // Arrange
            var scanner = new ApiOperationHandlerExecutorBuilderScanner();

            // Act
            var operationAHandlers = scanner.FindHandlers(
                new ServiceCollection(),
                typeof(OperationA),
                Enumerable.Empty<Assembly>());

            var operationBHandlers = scanner.FindHandlers(
                new ServiceCollection(),
                typeof(OperationB),
                Enumerable.Empty<Assembly>());

            // Assert
            operationAHandlers.Should().NotBeEmpty();
            operationBHandlers.Should().NotBeEmpty();
        }

        [Test]
        public void When_Handler_Exists_In_Assembly_And_IoC_Single_Returned()
        {
            // Arrange
            var scanner = new ApiOperationHandlerExecutorBuilderScanner();

            // Act
            var services = new ServiceCollection();
            services.AddSingleton<IApiOperationHandler<OperationA>, ScanOperationHandler>();

            var operationAHandlers = scanner.FindHandlers(
                services,
                typeof(OperationA),
                Enumerable.Empty<Assembly>());

            // Assert
            operationAHandlers.Should().NotBeEmpty();
            operationAHandlers.Should().HaveCount(1);
        }

        [Test]
        public void When_Handler_Exists_In_IoC_Finds()
        {
            // Arrange
            var scanner = new ApiOperationHandlerExecutorBuilderScanner();

            var services = new ServiceCollection();
            services.AddSingleton<ScanOperationHandler, ScanOperationHandler>();

            var handlers = scanner.FindHandlers(
                services,
                typeof(OperationA),
                Enumerable.Empty<Assembly>());

            handlers.Should().NotBeEmpty();
        }

        [Test]
        public void When_Handler_Registered_As_Interface_Exists_In_IoC_Finds()
        {
            // Arrange
            var scanner = new ApiOperationHandlerExecutorBuilderScanner();

            var services = new ServiceCollection();
            services.AddSingleton<IApiOperationHandler<OperationA>, ScanOperationHandler>();

            var handlers = scanner.FindHandlers(
                services,
                typeof(OperationA),
                Enumerable.Empty<Assembly>());

            handlers.Should().NotBeEmpty();
            handlers.OfType<ApiOperationHandlerExecutorBuilder>().Should().Contain(b =>
                b.ApiOperationHandlerType == typeof(ScanOperationHandler) &&
                b.ToString().StartsWith("IoC"));
        }

        [Test]
        public async Task When_Specific_Handler_Exists_Then_Finds_In_Scan()
        {
            // Arrange
            var executor = TestApiOperationExecutor.CreateStandalone(o => o
                .WithOperation<OperationA>()
                .WithOperation<OperationB>());

            // Act
            var resultA = await executor.ExecuteWithNewScopeAsync(new OperationA());
            var resultB = await executor.ExecuteWithNewScopeAsync(new OperationB());

            // Assert
            var okResultA = resultA.ShouldBeOperationResultType<OkResult>();
            okResultA.Content.Should().Be("OperationA");

            var okResultB = resultB.ShouldBeOperationResultType<OkResult>();
            okResultB.Content.Should().Be("OperationB");
        }

        public class OperationA
        {
        }

        public class OperationB
        {
        }

        public class ScanOperationHandler : IApiOperationHandler<OperationA>, IApiOperationHandler<OperationB>
        {
            public ValueTask<object> Handle(OperationA operation, ApiOperationContext apiOperationContext)
            {
                return new ValueTask<object>("OperationA");
            }

            public ValueTask<object> Handle(OperationB operation, ApiOperationContext apiOperationContext)
            {
                return new ValueTask<object>("OperationB");
            }
        }
    }
}
