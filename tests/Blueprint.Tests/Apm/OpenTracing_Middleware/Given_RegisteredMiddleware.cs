using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Testing;
using Blueprint.Tests.Api;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenTracing.Mock;
using OpenTracing.Tag;
using OpenTracing.Util;

namespace Blueprint.Tests.Apm.OpenTracing_Middleware
{
    public class Given_RegisteredMiddleware
    {
        [SetUp]
        public void SetupTracer()
        {
            GlobalTracer.RegisterIfAbsent(new MockTracer());
        }

        [Test]
        public async Task When_OpenTracing_Added_Then_Response_Of_Handler_Returned()
        {
            // Arrange
            var toReturn = 12345;

            var handler = new TestApiOperationHandler<EmptyOperation>(toReturn);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .WithServices(s => s.AddSingleton(GlobalTracer.Instance))
                .Configure(p => p.AddOpenTracing()));

            // Act
            var context = executor.ContextFor<EmptyOperation>();
            var result = await executor.ExecuteAsync(context);

            // Assert
            var okResult = result.ShouldBeOperationResultType<OkResult>();
            okResult.Content.Should().Be(toReturn);
            handler.WasCalled.Should().BeTrue();
        }

        [Test]
        public async Task When_OpenTracing_Added_With_Http_Then_Response_Of_Handler_Returned()
        {
            // Arrange
            var toReturn = 12345;

            var handler = new TestApiOperationHandler<EmptyOperation>(toReturn);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .WithServices(s => s.AddSingleton(GlobalTracer.Instance))
                .Configure(p => p.AddHttp().AddOpenTracing()));

            // Act
            var context = executor.HttpContextFor<EmptyOperation>();
            var result = await executor.ExecuteAsync(context);

            // Assert
            var okResult = result.ShouldBeOperationResultType<OkResult>();
            okResult.Content.Should().Be(toReturn);
            handler.WasCalled.Should().BeTrue();
        }

        [Test]
        public async Task When_OpenTracing_Added_Then_Handler_Exception_Bubbled()
        {
            // Arrange
            var toThrow = new Exception("Oops");

            var handler = new TestApiOperationHandler<EmptyOperation>(toThrow);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .WithServices(s => s.AddSingleton(GlobalTracer.Instance))
                .Configure(p => p.AddHttp().AddOpenTracing()));

            // Act
            var context = executor.HttpContextFor<EmptyOperation>();
            var result = await executor.ExecuteAsync(context);

            // Assert
            var okResult = result.Should().BeOfType<UnhandledExceptionOperationResult>().Subject;
            okResult.Exception.Should().Be(toThrow);
            handler.WasCalled.Should().BeTrue();
        }

        [Test]
        public async Task When_OpenTracing_Added_Then_Sets_Component_Tag()
        {
            // Arrange
            var scope = GlobalTracer.Instance.BuildSpan("ParentSpan").StartActive();

            var handler = new TestApiOperationHandler<EmptyOperation>(12345);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .WithServices(s => s.AddSingleton(GlobalTracer.Instance))
                .Configure(p => p.AddOpenTracing()));

            // Act
            var context = executor.ContextFor<EmptyOperation>();
            await executor.ExecuteAsync(context);

            // Assert
            ((MockSpan)scope.Span).Tags[Tags.Component.Key].Should().Be(nameof(EmptyOperation));
        }

        [Test]
        public async Task When_OpenTracing_Added_Then_Sets_Success_False_On_Exception()
        {
            // Arrange
            var handler = new TestApiOperationHandler<EmptyOperation>(new Exception("Failure"));
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .WithServices(s => s.AddSingleton(GlobalTracer.Instance))
                .Configure(p => p.AddOpenTracing()));

            // Act
            var context = executor.ContextFor<EmptyOperation>();
            await executor.ExecuteAsync(context);

            // Assert
        }

        [Test]
        public async Task When_OpenTracing_Added_With_User_Then_Sets_User_Context_Id_From_Sub_Claim()
        {
            // Arrange
            var scope = GlobalTracer.Instance.BuildSpan("ParentSpan").StartActive();

            var handler = new TestApiOperationHandler<EmptyOperation>(12345);
            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .WithServices(s => s.AddSingleton(GlobalTracer.Instance))
                .Configure(p => p.AddOpenTracing())
                .Pipeline(p => p.AddAuth<TestUserAuthorisationContextFactory>()));

            // Act
            var context = executor.ContextFor<EmptyOperation>();
            await executor.ExecuteAsync(context);

            await executor.ExecuteWithAuth(
                new EmptyOperation(),
                new Claim("sub", "UserId12345"));

            // Assert
            ((MockSpan)scope.Span).Tags.Should().Contain("user.id", "UserId12345");
        }
    }
}
