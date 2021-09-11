using System.Threading.Tasks;
using Blueprint.Http;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace Blueprint.Tests.Http
{
    public class Given_Http_Variables
    {
        [HttpPost]
        [RootLink("/some-static-value")]
        public class HttpOperation
        {
            public void Invoke(
                HttpFeatureContext featureContext,
                HttpContext httpContext,
                HttpRequest request,
                HttpResponse response)
            {
                featureContext.Should().NotBeNull();
                httpContext.Should().NotBeNull();
                request.Should().NotBeNull();
                response.Should().NotBeNull();
            }
        }

        [Test]
        public async Task When_Operation_with_inline_handler_then_populates_variables()
        {
            // Arrange
            var executor = TestApiOperationExecutor
                .CreateHttp(o => o.WithOperation<HttpOperation>());

            // Act
            var result = await executor.ExecuteAsync<HttpOperation>();

            // Assert
            result.ShouldBeOperationResultType<NoResultOperationResult>();
        }
    }
}
