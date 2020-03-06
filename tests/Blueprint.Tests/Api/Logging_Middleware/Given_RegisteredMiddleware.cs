using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Api.Logging_Middleware
{
    public class Given_Logging_Middleware
    {
        [Test]
        public async Task When_LoggingMiddleware_Added_Then_Response_Of_Handler_Returned()
        {
            // Arrange
            var toReturn = 12345;

            var handler = new TestApiOperationHandler<EmptyOperation>(toReturn);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Pipeline(p => p.AddLogging()));

            // Act
            var context = executor.ContextFor<EmptyOperation>();
            var result = await executor.ExecuteAsync(context);

            // Assert
            var okResult = result.Should().BeOfType<OkResult>().Subject;
            okResult.Content.Should().Be(toReturn);
            handler.WasCalled.Should().BeTrue();
        }
    }
}
