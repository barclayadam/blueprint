using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Testing;
using Blueprint.Tests.Api;
using Datadog.Trace;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Apm.DataDog_Middleware
{
    public class Given_RegisteredMiddleware
    {
        [SetUp]
        public void SetUpGlobalTracer()
        {
            Tracer.Instance = Tracer.Create();
        }

        [Test]
        public async Task When_Datadog_Added_Then_Response_Of_Handler_Returned()
        {
            // Arrange
            var toReturn = 12345;

            var handler = new TestApiOperationHandler<EmptyOperation>(toReturn);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Configure(p => p.AddDataDog()));

            // Act
            var context = executor.ContextFor<EmptyOperation>();
            var result = await executor.ExecuteAsync(context);

            // Assert
            var okResult = result.ShouldBeOperationResultType<OkResult>();
            okResult.Content.Should().Be(toReturn);
            handler.WasCalled.Should().BeTrue();
        }

        [Test]
        public async Task When_Datadog_Added_With_Http_Then_Response_Of_Handler_Returned()
        {
            // Arrange
            var toReturn = 12345;

            var handler = new TestApiOperationHandler<EmptyOperation>(toReturn);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Configure(p => p.AddHttp().AddDataDog()));

            // Act
            var context = executor.HttpContextFor<EmptyOperation>();
            var result = await executor.ExecuteAsync(context);

            // Assert
            var okResult = result.ShouldBeOperationResultType<OkResult>();
            okResult.Content.Should().Be(toReturn);
            handler.WasCalled.Should().BeTrue();
        }

        [Test]
        public async Task When_Datadog_Added_Then_Handler_Exception_Bubbled()
        {
            // Arrange
            var toThrow = new Exception("Oops");

            var handler = new TestApiOperationHandler<EmptyOperation>(toThrow);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Configure(p => p.AddHttp().AddDataDog()));

            // Act
            var context = executor.HttpContextFor<EmptyOperation>();
            var result = await executor.ExecuteAsync(context);

            // Assert
            var okResult = result.Should().BeOfType<UnhandledExceptionOperationResult>().Subject;
            okResult.Exception.Should().Be(toThrow);
            handler.WasCalled.Should().BeTrue();
        }

        [Test]
        public async Task When_Datadog_Added_Then_Sets_Resource()
        {
            // Arrange
            var scope = Tracer.Instance.StartActive("ParentSpan");

            var handler = new TestApiOperationHandler<EmptyOperation>(12345);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Configure(p => p.AddDataDog()));

            // Act
            var context = executor.ContextFor<EmptyOperation>();
            await executor.ExecuteAsync(context);

            // Assert
            scope.Span.ResourceName.Should().Be(nameof(EmptyOperation));
        }

        [Test]
        public async Task When_Datadog_Added_Then_Sets_Exception_On_Span()
        {
            // Arrange
            var scope = Tracer.Instance.StartActive("ParentSpan");

            var handler = new TestApiOperationHandler<EmptyOperation>(new Exception("Failure"));
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Configure(p => p.AddDataDog()));

            // Act
            var context = executor.ContextFor<EmptyOperation>();
            await executor.ExecuteAsync(context);

            // Assert
            scope.Span.Error.Should().BeTrue();
        }

        [Test]
        public async Task When_Datadog_Added_With_User_Then_Sets_User_Context_Id_From_Sub_Claim()
        {
            // Arrange
            var scope = Tracer.Instance.StartActive("ParentSpan");

            var handler = new TestApiOperationHandler<EmptyOperation>(12345);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .Configure(p => p.AddDataDog())
                .Pipeline(p => p.AddAuth<TestUserAuthorisationContextFactory>()));

            // Act
            var context = executor.ContextFor<EmptyOperation>();
            await executor.ExecuteAsync(context);

            await executor.ExecuteWithAuth(
                new EmptyOperation(),
                new Claim("sub", "UserId12345"));

            // Assert
            scope.Span.GetTag("user.id").Should().Be("UserId12345");
        }
    }
}
