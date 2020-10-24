using System.Threading.Tasks;
using Blueprint.Http;
using Blueprint.OpenApi;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.OpenApi
{
    public class Given_OpenApi_BrowserRequest
    {
        [Test]
        public async Task When_browser_request_returns_Refit_html()
        {
            // Arrange
            var executor = TestApiOperationExecutor
                .CreateHttp(o => o
                .AddOpenApi());

            // Act
            var context = executor.HttpContextFor<OpenApiQuery>(c =>
            {
                c.Request.Headers["Accept"] = "text/html";
            });

            var result = await executor.ExecuteAsync(context);

            // Assert
            var plaintextResult = result.ShouldBeOperationResultType<PlainTextResult>();

            plaintextResult.ContentType.Should().Be("text/html");
            plaintextResult.Content.Should().Contain("redoc");
        }

        [Test]
        public async Task When_browser_request_with_json_query_key_returns_json()
        {
            // Arrange
            var executor = TestApiOperationExecutor
                .CreateHttp(o => o
                    .AddOpenApi());

            // Act
            var context = executor.HttpContextFor<OpenApiQuery>(c =>
            {
                c.Request.Headers["Accept"] = "text/html";
                c.Request.QueryString = new QueryString("?json");
            });

            var result = await executor.ExecuteAsync(context);

            // Assert
            var plaintextResult = result.ShouldBeOperationResultType<PlainTextResult>();

            plaintextResult.ContentType.Should().Be("application/json");
        }
    }
}
