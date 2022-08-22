using System.Threading.Tasks;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.OperationExecutorBuilders;

public class Given_OperationHandler
{
    [Test]
    public async Task When_Handler_Already_Registered_Then_Used()
    {
        // Arrange
        var handler = new TestApiOperationHandler<TestApiCommand>("1234");
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithHandler(handler));

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
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithOperation<ScanOperation>());

        // Act
        var result = await executor.ExecuteWithNewScopeAsync(new ScanOperation());

        // Assert
        var okResult = result.ShouldBeOperationResultType<OkResult>();
        okResult.Content.Should().Be("6789");
    }

    public class ScanOperation
    {
    }

    public class ScanOperationHandler : IApiOperationHandler<ScanOperation>
    {
        public ValueTask<object> Handle(ScanOperation operation, ApiOperationContext apiOperationContext)
        {
            return new ValueTask<object>("6789");
        }
    }
}