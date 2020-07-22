using System.Threading.Tasks;
using Blueprint.Configuration;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Api.Builder
{
    public class Given_LoggingMiddleware
    {
        [Test]
        public async Task When_Generic_Operation_Type_Can_Compile()
        {
            // Arrange
            var handler = new TestApiOperationHandler<GenericOperation<TestApiCommand>>("12345");
            var executor = TestApiOperationExecutor.CreateStandalone(o =>
                o
                    .WithHandler(handler)
                    .AddLogging());

            // Act
            var result = await executor.ExecuteWithNewScopeAsync(new GenericOperation<TestApiCommand>
            {
                Operation = new TestApiCommand(),
            });

            // Assert
            result.Should().NotBeNull();
        }

        public class GenericOperation<T>
        {
            public T Operation { get; set; }
        }
    }
}
