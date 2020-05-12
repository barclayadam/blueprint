using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Api.Http;
using Blueprint.OpenApi;
using Blueprint.Testing;
using FluentAssertions;
using NSwag;
using NUnit.Framework;
using Snapper;
using Snapper.Attributes;

namespace Blueprint.Tests.Api.OpenApi_Tests
{
    // [UpdateSnapshots]
    public class Given_OpenApiQuery
    {
        [Test]
        public async Task When_no_operations_then_renders_correctly()
        {
            // Arrange
            var executor = TestApiOperationExecutor.Create(o => o
                .Configure(p => p.AddHttp().AddOpenApi()));

            // Act
            var context = executor.HttpContextFor<OpenApiQuery>();
            var result = await executor.ExecuteAsync(context);

            // Assert
            var plaintextResult = result.ShouldBeOperationResultType<PlainTextResult>();
            var openApiDocument = await OpenApiDocument.FromJsonAsync(plaintextResult.Content);

            openApiDocument.Info.Should().NotBeNull();
            openApiDocument.Components.Schemas.Should().BeEmpty();
            openApiDocument.Paths.Should().BeEmpty();

            plaintextResult.Content.ShouldMatchSnapshot();
        }

        [Test]
        public async Task When_OpenApi_with_operation_then_renders_correctly()
        {
            // Arrange
            var executor = TestApiOperationExecutor.Create(o => o
                .WithOperation<OpenApiGetQuery>()
                .Configure(p => p.AddHttp().AddOpenApi()));

            // Act
            var context = executor.HttpContextFor<OpenApiQuery>();
            var result = await executor.ExecuteAsync(context);

            // Assert
            var plaintextResult = result.ShouldBeOperationResultType<PlainTextResult>();
            var openApiDocument = await OpenApiDocument.FromJsonAsync(plaintextResult.Content);

            openApiDocument.Paths.Should().NotBeEmpty();
            plaintextResult.Content.ShouldMatchSnapshot();
        }

        [Test]
        public async Task When_OpenApi_with_operations_same_url_then_renders_correctly()
        {
            // Arrange
            var executor = TestApiOperationExecutor.Create(o => o
                .WithOperation<OpenApiGetQuery>()
                .WithOperation<OpenApiPutCommand>()
                .Configure(p => p.AddHttp().AddOpenApi()));

            // Act
            var context = executor.HttpContextFor<OpenApiQuery>();
            var result = await executor.ExecuteAsync(context);

            // Assert
            var plaintextResult = result.ShouldBeOperationResultType<PlainTextResult>();
            var openApiDocument = await OpenApiDocument.FromJsonAsync(plaintextResult.Content);

            openApiDocument.Paths.Should().NotBeEmpty();
            plaintextResult.Content.ShouldMatchSnapshot();
        }

        /// <summary>
        /// The OpenApiGetQuery summary
        /// </summary>
        [RootLink("/resources/{AnId}")]
        public class OpenApiGetQuery : IQuery<OpenApiResource>
        {
            public string AnId { get; set; }

            /// <summary>
            /// With some more documentation.
            /// </summary>
            [FromCookie]
            public string ACookieValue { get; set; }

            /// <summary>
            /// With some documentation.
            /// </summary>
            [FromHeader]
            public string AHeaderValue { get; set; }

            public OpenApiResource Invoke()
            {
                return new OpenApiResource();
            }
        }

        /// <summary>
        /// The OpenApiPutCommand summary
        /// </summary>
        [Link(typeof(OpenApiResource), "/resources/{AnId}", Rel = "update")]
        [HttpPut]
        public class OpenApiPutCommand : ICommand<ResourceUpdated<OpenApiResource>>
        {
            [Required]
            public string AnId { get; set; }

            public ResourceUpdated<OpenApiResource> Invoke()
            {
                return new ResourceUpdated<OpenApiResource>(new OpenApiGetQuery());
            }
        }

        public class OpenApiResource : ApiResource
        {
            public string AProperty { get; set; }
        }
    }
}
