using System;
using System.ComponentModel.DataAnnotations;
using System.Security;
using System.Threading.Tasks;
using Blueprint.Api.Configuration;
using Blueprint.Configuration;
using Blueprint.Errors;
using Blueprint.Http;
using Blueprint.OpenApi;
using Blueprint.Testing;
using FluentAssertions;
using NSwag;
using NUnit.Framework;
using Snapper;
using Snapper.Attributes;

namespace Blueprint.Tests.Api.OpenApi_Tests
{
    [UpdateSnapshots]
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
        public async Task When_GET_operation_then_renders_correctly()
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
        public async Task When_POST_operation_then_renders_correctly()
        {
            // Arrange
            var executor = TestApiOperationExecutor.Create(o => o
                .WithOperation<OpenApiPostCommand>()
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
        public async Task When_PlaintextResponse_result_operation_then_renders_correctly()
        {
            // Arrange
            // Add 'BasicOpenApiGetQuery' so that we output x-links to a PlaintextResult operation
            var executor = TestApiOperationExecutor.Create(o => o
                .WithOperation<BasicOpenApiGetQuery>()
                .WithOperation<OpenApiPlaintextResponseCommand>()
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
        public async Task When_operations_same_url_then_renders_correctly()
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

        // If a linked operation exists that would not have a body a "Collection was modified; enumeration operation may not execute."
        // exception thrown prior to 14/05/2020
        [Test]
        public async Task When_operation_link_exists_with_query_only_properties_renders_correctly()
        {
            // Arrange
            var executor = TestApiOperationExecutor.Create(o => o
                .WithOperation<OpenApiGetQuery>()
                .WithOperation<LinkedQueryWithQueryOnlyProperties>()
                .WithOperation<LinkedCommandWithNoBody>()
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
        /// <remarks>
        /// Some more remarks about this query.
        /// </remarks>
        /// <exception cref="NotFoundException">When needed (will be 404).</exception>
        /// <exception cref="InvalidOperationException">When needed (will be 400).</exception>
        /// <exception cref="ForbiddenException">When needed (will be 403).</exception>
        /// <exception cref="SecurityException">When needed (will be 401).</exception>
        /// <exception cref="ApplicationException">When needed (will be 500).</exception>
        /// <exception cref="ApiException" type="some_error_type" status="400">Invalid op (will be 400).</exception>
        /// <exception cref="ApiException" type="another_specific_problem_detail" status="404">Missing data (will be 404).</exception>
        [RootLink("/resources/{AnId}")]
        public class OpenApiGetQuery : IQuery<OpenApiResource>
        {
            public static readonly ApiExceptionFactory OtherMissingData = new ApiExceptionFactory(
                "Missing data again (will be 404)",
                "other_missing_data",
                404);

            public string AnId { get; set; }

            public string ImplicitlyFromQuery { get; set; }

            [FromQuery("DifferentNameForQuery")]
            public string ExplicitlyFromQuery { get; set; }

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

        [RootLink("/resources/basic")]
        public class BasicOpenApiGetQuery : IQuery<OpenApiResource>
        {
            public OpenApiResource Invoke()
            {
                return new OpenApiResource();
            }
        }

        /// <summary>
        /// The OpenApiPostCommand summary
        /// </summary>
        [RootLink("/resources/{AnId}")]
        public class OpenApiPostCommand : ICommand<ResourceCreated<OpenApiResource>>
        {
            public string AnId { get; set; }

            public string ABodyParameter { get; set; }

            [Required]
            public string AnotherBodyParameter { get; set; }

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

            public ResourceCreated<OpenApiResource> Invoke()
            {
                return new ResourceCreated<OpenApiResource>(new OpenApiGetQuery());
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

        /// <summary>
        /// The OpenApiPlaintextResponse summary
        /// </summary>
        [Link(typeof(OpenApiResource), "/resources/{AnId}/as-plaintext", Rel = "plaintext-rel")]
        [HttpPut]
        public class OpenApiPlaintextResponseCommand : ICommand<PlainTextResult>
        {
            [Required]
            public string AnId { get; set; }

            public PlainTextResult Invoke()
            {
                return new PlainTextResult("The content to respond with");
            }
        }

        /// <summary>
        /// The LinkedQueryWithQueryOnlyProperties summary
        /// </summary>
        [Link(typeof(OpenApiResource), "/resources/a-static-query-url", Rel = "no-body-query")]
        public class LinkedQueryWithQueryOnlyProperties : IQuery<OpenApiResource>
        {
            [Required]
            public string AnId { get; set; }

            public OpenApiResource Invoke()
            {
                return new OpenApiResource();
            }
        }

        /// <summary>
        /// The LinkedCommandWithNoBody summary
        /// </summary>
        [Link(typeof(OpenApiResource), "/resources/a-static-command-url", Rel = "no-body-command")]
        public class LinkedCommandWithNoBody : ICommand<OpenApiResource>
        {
            [Required]
            [FromQuery]
            public string AnId { get; set; }

            public OpenApiResource Invoke()
            {
                return new OpenApiResource();
            }
        }

        public class OpenApiResource : ApiResource
        {
            public string AProperty { get; set; }
        }
    }
}
