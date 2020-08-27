using System.Threading.Tasks;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.OperationExecutorBuilders
{
    public class Given_Multiple_ApiOperationHandlers_Same_Class
    {
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
