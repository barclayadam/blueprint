﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using Blueprint.Errors;
using Blueprint.Http;
using Blueprint.OpenApi;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NUnit.Framework;
using Snapper;
using Snapper.Attributes;

namespace Blueprint.Tests.OpenApi
{
    // [UpdateSnapshots]
    public class Given_OpenApiQuery
    {
        [Test]
        public async Task When_called_multiple_times_servers_updated_correctly()
        {
            // Bug report of servers[0].url being wrong on subsequent calls due to how setting BasePath works in NSwag.

            // Arrange
            var executor = TestApiOperationExecutor.CreateHttp(o => o
                .AddOpenApi());

            // Act
            var context = executor.HttpContextFor<OpenApiQuery>();

            var result1 = await executor.ExecuteAsync(context);
            var result2 = await executor.ExecuteAsync(context);

            // Assert
            var plaintextResult1 = result1.ShouldBeOperationResultType<PlainTextResult>();
            var plaintextResult2 = result2.ShouldBeOperationResultType<PlainTextResult>();

            plaintextResult1.Content.Should().Be(plaintextResult2.Content);
        }

        [Test]
        public async Task When_no_operations_then_renders_correctly()
        {
            // Arrange
            var executor = TestApiOperationExecutor.CreateHttp(o => o
                .AddOpenApi());

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
        public async Task When_resource_with_HttpStatusCode_then_renders_correctly()
        {
            // Arrange
            var executor = TestApiOperationExecutor.CreateHttp(o => o
                .WithOperation<HttpStatusCodeQuery>()
                .AddOpenApi());

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
        public async Task When_specific_StatusCodeResult_then_renders_correctly()
        {
            // Arrange
            var executor = TestApiOperationExecutor.CreateHttp(o => o
                .WithOperation<CreatedStatusCodeQuery>()
                .AddOpenApi());

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
        public async Task When_base_StatusCodeResult_then_renders_correctly()
        {
            // Arrange
            var executor = TestApiOperationExecutor.CreateHttp(o => o
                .WithOperation<BaseStatusCodeQuery>()
                .AddOpenApi());

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
        public async Task When_GET_operation_then_renders_correctly()
        {
            // Arrange
            var executor = TestApiOperationExecutor.CreateHttp(o => o
                .WithOperation<OpenApiGetQuery>()
                .AddOpenApi());

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
        public async Task When_enumerable_result_then_renders_correctly()
        {
            // Arrange
            var executor = TestApiOperationExecutor.CreateHttp(o => o
                .WithOperation<EnumerableOpenApiGetQuery>()
                .AddOpenApi());

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
        public async Task When_PagedApiResponse_result_then_renders_correctly()
        {
            // Arrange
            var executor = TestApiOperationExecutor.CreateHttp(o => o
                .WithOperation<PagedOpenApiGetQuery>()
                .AddOpenApi());

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
            var executor = TestApiOperationExecutor.CreateHttp(o => o
                .WithOperation<OpenApiPostCommand>()
                .AddOpenApi());

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
            var executor = TestApiOperationExecutor.CreateHttp(o => o
                .WithOperation<BasicOpenApiGetQuery>()
                .WithOperation<OpenApiPlaintextResponseCommand>()
                .AddOpenApi());

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
            var executor = TestApiOperationExecutor.CreateHttp(o => o
                .WithOperation<OpenApiGetQuery>()
                .WithOperation<OpenApiPutCommand>()
                .AddOpenApi());

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
            var executor = TestApiOperationExecutor.CreateHttp(o => o
                .WithOperation<OpenApiGetQuery>()
                .WithOperation<LinkedQueryWithQueryOnlyProperties>()
                .WithOperation<LinkedCommandWithNoBody>()
                .AddOpenApi());

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

        [RootLink("/resources/status-code")]
        public class HttpStatusCodeQuery : IQuery<ResourceWithHttpStatusCode>
        {
            public ResourceWithHttpStatusCode Invoke()
            {
                return new ResourceWithHttpStatusCode();
            }
        }

        [RootLink("/resources/status-code")]
        public class CreatedStatusCodeQuery : IQuery<StatusCodeResult.Created>
        {
            public StatusCodeResult.Created Invoke()
            {
                return StatusCodeResult.Created.Instance;
            }
        }

        [RootLink("/resources/status-code")]
        public class BaseStatusCodeQuery : IQuery<StatusCodeResult>
        {
            public StatusCodeResult Invoke()
            {
                return StatusCodeResult.Created.Instance;
            }
        }

        public class ResourceWithHttpStatusCode : ApiResource
        {
            public HttpStatusCode Code { get; set; }
        }

        [RootLink("/resources/basic")]
        public class BasicOpenApiGetQuery : IQuery<OpenApiResource>
        {
            public OpenApiResource Invoke()
            {
                return new OpenApiResource();
            }
        }

        [RootLink("/resources/enumerable")]
        public class EnumerableOpenApiGetQuery : IQuery<IEnumerable<OpenApiResource>>
        {
            public IEnumerable<OpenApiResource> Invoke()
            {
                return new [] { new OpenApiResource() };
            }
        }

        [RootLink("/resources/paged")]
        public class PagedOpenApiGetQuery : IQuery<PagedApiResource<OpenApiResource>>
        {
            public PagedApiResource<OpenApiResource> Invoke()
            {
                return new PagedApiResource<OpenApiResource>(new [] { new OpenApiResource() });
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
