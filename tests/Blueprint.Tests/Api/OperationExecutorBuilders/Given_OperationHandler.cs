using System.Threading.Tasks;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Api.OperationExecutorBuilders
{
    public class Given_OperationHandler
    {
        [Test]
        public async Task When_Handler_Already_Registered_Then_Used()
        {
            // Arrange
            var handler = new TestApiOperationHandler<TestApiCommand>("1234");
            var executor = TestApiOperationExecutor.Create(o => o.WithHandler(handler));

            // Act
            var result = await executor.ExecuteWithNewScopeAsync(new TestApiCommand
            {
                AStringProperty = "the string value",
                ASensitiveStringProperty = "the sensitive value"
            });

            // Assert
            var okResult = result.ShouldBeOperationResultType<OkResult>();
            okResult.Content.Should().Be("1234");
        }

        [Test]
        public async Task When_Specific_Handler_Exists_Then_Finds_In_Scan()
        {
            // Arrange
            var executor = TestApiOperationExecutor.Create(o => o.WithOperation<ScanOperation>());

            // Act
            var result = await executor.ExecuteWithNewScopeAsync(new ScanOperation());

            // Assert
            var okResult = result.ShouldBeOperationResultType<OkResult>();
            okResult.Content.Should().Be("6789");
        }

        public class ScanOperation : IApiOperation
        {
        }

        public class ScanOperationHandler : IApiOperationHandler<ScanOperation>
        {
            public Task<object> Invoke(ScanOperation operation, ApiOperationContext apiOperationContext)
            {
                return Task.FromResult((object)"6789");
            }
        }
    }
}
